using Dicom.Network;

namespace CMoveSCP
{
    /// <summary>
    /// Classe permettant d'obtenir les chemins des images � envoyer
    /// correspondant � une requ�te Cmove
    /// </summary>
    public interface ICMoveImageFinder
    {
        /// <summary>
        /// permet d'obtenir les chemins des images � envoyer
        /// correspondant � la requ�te cMoveRequest
        /// </summary>
        /// <param name="cMoveRequest"></param>
        /// <returns></returns>
        string[] GetImagesFilePathsToSend(DicomCMoveRequest cMoveRequest);
    }
}