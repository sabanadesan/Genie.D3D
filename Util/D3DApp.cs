using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Input;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D12;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D12.Device;
using Feature = SharpDX.Direct3D12.Feature;
using Point = SharpDX.Point;
using Resource = SharpDX.Direct3D12.Resource;
using RectangleF = SharpDX.RectangleF;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace D3D
{
    // TODO: There are currently following standing issues with all the samples:
    // TODO: Entering fullscreen mode will crash - https://github.com/d3dcoder/d3d12book/issues/2
    // TODO: Changing multisample settings will crash - https://github.com/d3dcoder/d3d12book/issues/3
    public class D3DApp : IDisposable
    {
        public const int NumFrameResources = 3;
        public const int SwapChainBufferCount = 2;

        System.Object swapChainPanel;
        private int frameIndex;
        const int FrameCount = 2;

        private bool _appPaused;          // Is the application paused?
        private bool _minimized;          // Is the application minimized?
        private bool _maximized;          // Is the application maximized?
        private bool _resizing;           // Are the resize bars being dragged?
        protected bool _running;            // Is the application running?

        // Set true to use 4X MSAA (§4.1.8).
        private bool _m4xMsaaState;       // 4X MSAA enabled.
        private int _m4xMsaaQuality;      // Quality level of 4X MSAA.

        private int _frameCount;
        private float _timeElapsed;

        private Factory1 _factory;
        private readonly Resource[] _swapChainBuffers = new Resource[SwapChainBufferCount];

        private AutoResetEvent _fenceEvent;

        public bool M4xMsaaState
        {
            get { return _m4xMsaaState; }
            set
            {
                if (_m4xMsaaState != value)
                {
                    _m4xMsaaState = value;

                    if (_running)
                    {
                        // Recreate the swapchain and buffers with new multisample settings.
                        CreateSwapChain();
                        OnResize();
                    }
                }
            }
        }

        protected DescriptorHeap RtvHeap { get; private set; }
        protected DescriptorHeap DsvHeap { get; private set; }

        protected int MsaaCount => M4xMsaaState ? 4 : 1;
        protected int MsaaQuality => M4xMsaaState ? _m4xMsaaQuality - 1 : 0;

        protected GameTimer Timer { get; } = new GameTimer();

        protected Device Device { get; private set; }

        protected Fence Fence { get; private set; }
        protected long CurrentFence { get; set; }

        protected int RtvDescriptorSize { get; private set; }
        protected int DsvDescriptorSize { get; private set; }
        protected int CbvSrvUavDescriptorSize { get; private set; }

        protected CommandQueue CommandQueue { get; private set; }
        protected CommandAllocator DirectCmdListAlloc { get; private set; }
        protected GraphicsCommandList CommandList { get; private set; }

        protected SwapChain3 SwapChain { get; private set; }
        protected Resource DepthStencilBuffer { get; private set; }

        protected ViewportF Viewport { get; set; }
        protected RectangleF ScissorRectangle { get; set; }

        protected string MainWindowCaption { get; set; } = "D3D12 Application";
        protected int ClientWidth { get; set; } = 1280;
        protected int ClientHeight { get; set; } = 720;
        
        protected float AspectRatio => (float)ClientWidth / ClientHeight;

        protected Format BackBufferFormat { get; } = Format.R8G8B8A8_UNorm;
        protected Format DepthStencilFormat { get; } = Format.D24_UNorm_S8_UInt;

        protected Resource CurrentBackBuffer => _swapChainBuffers[SwapChain.CurrentBackBufferIndex];
        protected CpuDescriptorHandle CurrentBackBufferView
            => RtvHeap.CPUDescriptorHandleForHeapStart + SwapChain.CurrentBackBufferIndex * RtvDescriptorSize;
        protected CpuDescriptorHandle DepthStencilView => DsvHeap.CPUDescriptorHandleForHeapStart;

        protected CancellationTokenSource CancellationTokenSource;

        long lastFrame;

        private TextBox fpstext;
        private TextBox counttext;
        float fps;
        float mspf;
        int count;

        public TextBox FpsTextBox
        {
            get
            {
                return fpstext;
            }
            set
            {
                fpstext = value;
            }
        }
        public TextBox CountTextBox
        {
            get { return counttext; }
            set
            {
                counttext = value;
            }
        }

        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
            }
        }

        public bool IsRunning
        {
            get { return _running; }
        }

        public D3DApp(System.Object swapChainPanel)
        {
            this.swapChainPanel = swapChainPanel;
            count = 0;
            lastFrame = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public virtual void Initialize()
        {           
            InitDirect3D();

            // Do the initial resize code.
            OnResize();

            _running = true;
        }

        public bool Run(Action action, CancellationTokenSource token)
        {
            CancellationTokenSource = token;

            Timer.Reset();
            while (!CancellationTokenSource.IsCancellationRequested)
            {
                Timer.Tick();
                DrawUI();
                OnResize();
                action();
                CalculateFrameRateStats();
                Update(Timer);
                Draw(Timer);
                Sleep();
            }

            if (!_running)
            {
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Implements the basic dispose pattern.
            // Ref: https://msdn.microsoft.com/en-us/library/b1yfkh5e(v=vs.110).aspx
            if (disposing)
            {
                FlushCommandQueue();

                RtvHeap?.Dispose();
                DsvHeap?.Dispose();
                SwapChain?.Dispose();
                foreach (Resource buffer in _swapChainBuffers)
                    buffer?.Dispose();
                DepthStencilBuffer?.Dispose();
                CommandList?.Dispose();
                DirectCmdListAlloc?.Dispose();
                CommandQueue?.Dispose();
                Fence?.Dispose();
                Device?.Dispose();
            }
        }

        protected virtual void OnResize()
        {
            Debug.Assert(Device != null);
            Debug.Assert(SwapChain != null);
            Debug.Assert(DirectCmdListAlloc != null);

            // Flush before changing any resources.
            FlushCommandQueue();

            CommandList.Reset(DirectCmdListAlloc, null);

            // Release the previous resources we will be recreating.
            foreach (Resource buffer in _swapChainBuffers)
                buffer?.Dispose();
            DepthStencilBuffer?.Dispose();

            // Resize the swap chain.
            SwapChain.ResizeBuffers(
                SwapChainBufferCount,
                ClientWidth, ClientHeight,
                BackBufferFormat,
                SwapChainFlags.AllowModeSwitch);

            CpuDescriptorHandle rtvHeapHandle = RtvHeap.CPUDescriptorHandleForHeapStart;
            for (int i = 0; i < SwapChainBufferCount; i++)
            {
                Resource backBuffer = SwapChain.GetBackBuffer<Resource>(i);
                _swapChainBuffers[i] = backBuffer;
                Device.CreateRenderTargetView(backBuffer, null, rtvHeapHandle);
                rtvHeapHandle += RtvDescriptorSize;
            }

            // Create the depth/stencil buffer and view.
            var depthStencilDesc = new ResourceDescription
            {
                Dimension = ResourceDimension.Texture2D,
                Alignment = 0,
                Width = ClientWidth,
                Height = ClientHeight,
                DepthOrArraySize = 1,
                MipLevels = 1,
                Format = Format.R24G8_Typeless,
                SampleDescription = new SampleDescription
                {
                    Count = MsaaCount,
                    Quality = MsaaQuality
                },
                Layout = TextureLayout.Unknown,
                Flags = ResourceFlags.AllowDepthStencil
            };
            var optClear = new ClearValue
            {
                Format = DepthStencilFormat,
                DepthStencil = new DepthStencilValue
                {
                    Depth = 1.0f,
                    Stencil = 0
                }
            };
            DepthStencilBuffer = Device.CreateCommittedResource(
                new HeapProperties(HeapType.Default),
                HeapFlags.None,
                depthStencilDesc,
                ResourceStates.Common,
                optClear);

            var depthStencilViewDesc = new DepthStencilViewDescription
            {
                Dimension = M4xMsaaState 
                    ? DepthStencilViewDimension.Texture2DMultisampled
                    : DepthStencilViewDimension.Texture2D,
                Format = DepthStencilFormat
            };
            // Create descriptor to mip level 0 of entire resource using a depth stencil format.
            CpuDescriptorHandle dsvHeapHandle = DsvHeap.CPUDescriptorHandleForHeapStart;
            Device.CreateDepthStencilView(DepthStencilBuffer, depthStencilViewDesc, dsvHeapHandle);

            // Transition the resource from its initial state to be used as a depth buffer.
            CommandList.ResourceBarrierTransition(DepthStencilBuffer, ResourceStates.Common, ResourceStates.DepthWrite);

            // Execute the resize commands.
            CommandList.Close();
            CommandQueue.ExecuteCommandList(CommandList);

            // Wait until resize is complete.
            FlushCommandQueue();

            Viewport = new ViewportF(0, 0, ClientWidth, ClientHeight, 0.0f, 1.0f);
            ScissorRectangle = new RectangleF(0, 0, ClientWidth, ClientHeight);
        }

        protected virtual void Update(GameTimer gt) { }
        protected virtual void Draw(GameTimer gt) { }

        protected void InitDirect3D()
        {
#if DEBUG
            // The Direct3D 12 debug layer may or may not be installed. It's installation can be
            // managed through settings page "Manage optional features" with a feature called
            // "Graphics Tools".
            // There may be a better solution to check for it instead of try/catch. If you happen
            // to know, please consider opening an issue or PR in the repo.
            try
            {
                DebugInterface.Get().EnableDebugLayer();
            }
            catch (SharpDXException ex) when (ex.Descriptor.NativeApiCode == "DXGI_ERROR_SDK_COMPONENT_MISSING")
            {
                Debug.WriteLine("Failed to enable debug layer. Please ensure \"Graphics Tools\" feature is enabled in Windows \"Manage optional feature\" settings page");
            }
#endif

            _factory = new SharpDX.DXGI.Factory1();
            SharpDX.DXGI.Adapter adapter = _factory.GetAdapter(1);

            // create device
            using (var defaultDevice = new Device(adapter, SharpDX.Direct3D.FeatureLevel.Level_12_1))
                Device = defaultDevice.QueryInterface<SharpDX.Direct3D12.Device2>();

            Fence = Device.CreateFence(0, FenceFlags.None);
            _fenceEvent = new AutoResetEvent(false);

            RtvDescriptorSize = Device.GetDescriptorHandleIncrementSize(DescriptorHeapType.RenderTargetView);
            DsvDescriptorSize = Device.GetDescriptorHandleIncrementSize(DescriptorHeapType.DepthStencilView);
            CbvSrvUavDescriptorSize = Device.GetDescriptorHandleIncrementSize(
                DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);

            // Check 4X MSAA quality support for our back buffer format.
            // All Direct3D 11 capable devices support 4X MSAA for all render 
            // target formats, so we only need to check quality support.

            FeatureDataMultisampleQualityLevels msQualityLevels;
            msQualityLevels.Format = BackBufferFormat;
            msQualityLevels.SampleCount = 4;
            msQualityLevels.Flags = MultisampleQualityLevelFlags.None;
            msQualityLevels.QualityLevelCount = 0;
            Debug.Assert(Device.CheckFeatureSupport(Feature.MultisampleQualityLevels, ref msQualityLevels));
            _m4xMsaaQuality = msQualityLevels.QualityLevelCount;

#if DEBUG
            LogAdapters();
#endif

            CreateCommandObjects();
            CreateSwapChain();
            CreateRtvAndDsvDescriptorHeaps();
        }

        protected void FlushCommandQueue()
        {
            // Advance the fence value to mark commands up to this fence point.
            CurrentFence++;

            // Add an instruction to the command queue to set a new fence point.  Because we
            // are on the GPU timeline, the new fence point won't be set until the GPU finishes
            // processing all the commands prior to this Signal().
            CommandQueue.Signal(Fence, CurrentFence);

            // Wait until the GPU has completed commands up to this fence point.
            if (Fence.CompletedValue < CurrentFence)
            {
                // Fire event when GPU hits current fence.
                Fence.SetEventOnCompletion(CurrentFence, _fenceEvent.SafeWaitHandle.DangerousGetHandle());

                // Wait until the GPU hits current fence event is fired.
                _fenceEvent.WaitOne();
            }
        }

        protected virtual int RtvDescriptorCount => SwapChainBufferCount;
        protected virtual int DsvDescriptorCount => 1;

        private void CreateCommandObjects()
        {
            var queueDesc = new CommandQueueDescription(CommandListType.Direct);
            CommandQueue = Device.CreateCommandQueue(queueDesc);

            DirectCmdListAlloc = Device.CreateCommandAllocator(CommandListType.Direct);

            CommandList = Device.CreateCommandList(
                0,
                CommandListType.Direct,
                DirectCmdListAlloc, // Associated command allocator.
                null);              // Initial PipelineStateObject.

            // Start off in a closed state.  This is because the first time we refer
            // to the command list we will Reset it, and it needs to be closed before
            // calling Reset.
            CommandList.Close();
        }

        private void CreateSwapChain()
        {
            // Release the previous swapchain we will be recreating.
            SwapChain?.Dispose();
            using (var factory = new SharpDX.DXGI.Factory4())
            {

                // Describe and create the swap chain.
                var sd = new SharpDX.DXGI.SwapChainDescription1()
                {
                    BufferCount = FrameCount,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    Width = ClientWidth,
                    Height = ClientHeight,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
                    Scaling = SharpDX.DXGI.Scaling.Stretch,
                    Stereo = false,
                    SwapEffect = SharpDX.DXGI.SwapEffect.FlipDiscard,
                    Usage = SharpDX.DXGI.Usage.RenderTargetOutput,
                };

                var tempSwapChain = new SharpDX.DXGI.SwapChain1(factory, CommandQueue, ref sd);
                SwapChain = tempSwapChain.QueryInterface<SharpDX.DXGI.SwapChain3>();
                tempSwapChain.Dispose();
                frameIndex = SwapChain.CurrentBackBufferIndex;

                using (SharpDX.DXGI.ISwapChainPanelNative nativeObject = ComObject.As<SharpDX.DXGI.ISwapChainPanelNative>(swapChainPanel))
                    nativeObject.SwapChain = SwapChain;
            }
        }

        private void CreateRtvAndDsvDescriptorHeaps()
        {
            var rtvHeapDesc = new DescriptorHeapDescription
            {
                DescriptorCount = RtvDescriptorCount,
                Type = DescriptorHeapType.RenderTargetView
            };
            RtvHeap = Device.CreateDescriptorHeap(rtvHeapDesc);

            var dsvHeapDesc = new DescriptorHeapDescription
            {
                DescriptorCount = DsvDescriptorCount,
                Type = DescriptorHeapType.DepthStencilView
            };
            DsvHeap = Device.CreateDescriptorHeap(dsvHeapDesc);
        }

        private void LogAdapters()
        {
            foreach (Adapter adapter in _factory.Adapters)
            {
                Debug.WriteLine($"***Adapter: {adapter.Description.Description}");
                LogAdapterOutputs(adapter);
            }
        }

        private void LogAdapterOutputs(Adapter adapter)
        {
            foreach (Output output in adapter.Outputs)
            {
                Debug.WriteLine($"***Output: {output.Description.DeviceName}");
                LogOutputDisplayModes(output, BackBufferFormat);
            }
        }

        private void LogOutputDisplayModes(Output output, Format format)
        {
            foreach (ModeDescription displayMode in output.GetDisplayModeList(format, 0))
                Debug.WriteLine($"Width = {displayMode.Width} Height = {displayMode.Height} Refresh = {displayMode.RefreshRate}");
        }

        private void CalculateFrameRateStats()
        {
            _frameCount++;

            if (Timer.TotalTime - _timeElapsed >= 1.0f)
            {
                fps = _frameCount;
                mspf = 1000.0f / fps;

                // Reset for next average.
                _frameCount = 0;
                _timeElapsed += 1.0f;
            }
        }

        private void DrawUI()
        {
            var ignored = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                FpsTextBox.Text = $"{MainWindowCaption}    fps: {fps}   mspf: {mspf}";
                CountTextBox.Text = $"Count: {count}";
            });
        }

        private void Sleep()
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            int delta = (int) (now - lastFrame);

            lastFrame = now;

            int fps = (int) (1000 / 30);

            if (delta < fps)
            {
                System.Threading.Thread.Sleep(fps - delta);
            }
        }
    }
}
