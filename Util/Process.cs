using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using System.Threading;
using System.Threading.Tasks;

namespace D3D
{
    public static class ProcessServer
    {
        private static Dictionary<String, Process> _registeredProcesses = new Dictionary<String, Process>();

        public static void Register(String key, Process t)
        {
            _registeredProcesses.Remove(key);
            _registeredProcesses.Add(key, t);
        }

        public static Process Resolve(String key)
        {
            return _registeredProcesses[key];
        }
    }

    public class Process
    {
        private String m_key;
        private Task m_t;
        private Do m_d;

        private CancellationTokenSource source1;
        private CancellationToken token1;

        public delegate void MyFunction(CancellationToken token);
        private MyFunction m;

        public Process(String key)
        {
            m_key = key;
        }

        public void Wait()
        {
            m_t.Wait();
        }

        public Task Run(Action<CancellationTokenSource> a)
        {
            source1 = new CancellationTokenSource();

            m_t = new Task(() =>
            {
                a(source1);
            }, token1);

            m_t.Start();

            ProcessServer.Register(m_key, this);

            return m_t;
        }

        public Task Run(MyFunction f)
        {
            source1 = new CancellationTokenSource();
            token1 = source1.Token;

            token1.Register(() =>
            {
                Console.WriteLine("Cancelled.");
            });

            m_t = new Task(() =>
            {
                f(token1);
            }, token1);

            m_t.Start();

            ProcessServer.Register(m_key, this);

            return m_t;
        }

        public Task Run(Do d)
        {
            source1 = new CancellationTokenSource();
            token1 = source1.Token;

            token1.Register(() =>
            {
                Console.WriteLine("Cancelled.");
            });

            m_d = d;

            m_t = new Task(() =>
            {
                m_d.Run(token1);
            }, token1);

            m_t.Start();

            ProcessServer.Register(m_key, this);

            return m_t;
        }

        public void Stop()
        {
            source1.Cancel();
            source1.Dispose();
        }
    }

    public abstract class Do
    {
        public abstract void Run(CancellationToken token);

        public abstract void Stop();

        public abstract void Start();
    }
}