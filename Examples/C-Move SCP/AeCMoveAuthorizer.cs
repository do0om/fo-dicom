namespace CMoveSCP
{

    /// <summary>
    /// IAeCMoveAuthorizer for testing purposes
    /// </summary>
    public class TestAeCMoveAuthorizer : IAeCMoveAuthorizer
    {
        public bool IsAeAllowedToReceiveCMove(string aeTitle,
            out ApplicationEntityNetworkInfos aeDestinationNetworkInfos)
        {
            //only ae called CMoveDestination is allowed to be the destination of a CMove
            if (aeTitle != "CMoveDestination")
            {
                aeDestinationNetworkInfos = new ApplicationEntityNetworkInfos(null, 0);
                return false;
            }

            aeDestinationNetworkInfos = new ApplicationEntityNetworkInfos("127.0.0.1", 11115);
            return true;
        }
    }

}