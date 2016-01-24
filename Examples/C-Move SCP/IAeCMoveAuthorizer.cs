namespace CMoveSCP
{
    /// <summary>
    /// Authorize an Aet to be the destination of a CMove
    /// </summary>
    public interface IAeCMoveAuthorizer
    {
        /// <summary>
        /// Gets a value indicating whether the Application Entity with title=aeTitle
        /// can receive a CMove.
        /// If result is true, aeDestinationNetworkInfos is filled with its network informations.
        /// If result is false, aeDestinationNetworkInfos has no meaning.
        /// </summary>
        /// <param name="aeTitle"></param>
        /// <param name="aeDestinationNetworkInfos"></param>
        /// <returns>true if aet is allowed, else false</returns>
        bool IsAeAllowedToReceiveCMove(string aeTitle,
            out ApplicationEntityNetworkInfos aeDestinationNetworkInfos);
    }
}