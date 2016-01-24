using Dicom.Network;

namespace CMoveSCP
{
    /// <summary>
    /// Classe permettant d'obtenir les chemins des images à envoyer
    /// correspondant à une requête Cmove
    /// </summary>
    public interface ICMoveImageFinder
    {
        /// <summary>
        /// permet d'obtenir les chemins des images à envoyer
        /// correspondant à la requête cMoveRequest
        /// </summary>
        /// <param name="cMoveRequest"></param>
        /// <returns></returns>
        string[] GetImagesFilePathsToSend(DicomCMoveRequest cMoveRequest);
    }
}