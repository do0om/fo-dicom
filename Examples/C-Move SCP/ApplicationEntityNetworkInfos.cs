namespace CMoveSCP
{
    /// <summary>
    /// Contains all informations to communicate with an entity on the network
    /// </summary>
    public class ApplicationEntityNetworkInfos
    {

        /// <summary>
        /// host name of ip address
        /// </summary>
        public string HostNameOrIp { get; private set; }

        /// <summary>
        /// port
        /// </summary>
        public short Port { get; private set; }

        /// <summary>
        /// Initialize a new instance of ApplicationEntityNetworkInfos
        /// </summary>
        /// <param name="hostNameOrIp"></param>
        /// <param name="port"></param>
        public ApplicationEntityNetworkInfos(string hostNameOrIp, short port)
        {
            HostNameOrIp = hostNameOrIp;
            Port = port;
        }
    }
}