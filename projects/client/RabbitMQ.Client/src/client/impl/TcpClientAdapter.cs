﻿using System;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace RabbitMQ.Client
{
    /// <summary>
    /// Simple wrapper around TcpClient.
    /// </summary>
    public class TcpClientAdapter : ITcpClient
    {
        private Socket _sock;

        public TcpClientAdapter(Socket socket)
        {
            if (socket == null)
                throw new InvalidOperationException("socket must not be null");

            _sock = socket;
        }

        public virtual async Task ConnectAsync(string host, int port)
        {
            AssertSocket();
            IPAddress[] adds = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
            IPAddress ep = TcpClientAdapterHelper.GetMatchingHost(adds, _sock.AddressFamily);
            if (ep == default(IPAddress))
            {
                throw new ArgumentException("No ip address could be resolved for " + host);
            }

#if CORECLR
            await _sock.ConnectAsync(ep, port).ConfigureAwait(false);
#else
            await Task.Run(() => _sock.Connect(ep, port));
#endif
        }

        public virtual void Close()
        {
            if (_sock != null)
            {
                _sock.Dispose();
            }
            _sock = null;
        }

        [Obsolete("Override Dispose(bool) instead.")]
        public virtual void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                Close();
            }

            // dispose unmanaged resources
        }

        public virtual NetworkStream GetStream()
        {
            AssertSocket();
            return new NetworkStream(_sock);
        }

        public virtual Socket Client
        {
            get
            {
                return _sock;
            }
        }

        public virtual bool Connected
        {
            get
            {
                if(_sock == null) return false;
                return _sock.Connected;
            }
        }

        public virtual TimeSpan ReceiveTimeout
        {
            get
            {
                AssertSocket();
                return TimeSpan.FromMilliseconds(_sock.ReceiveTimeout);
            }
            set
            {
                AssertSocket();
                _sock.ReceiveTimeout = (int)value.TotalMilliseconds;
            }
        }

        private void AssertSocket()
        {
            if(_sock == null)
            {
                throw new InvalidOperationException("Cannot perform operation as socket is null");
            }
        }
    }
}
