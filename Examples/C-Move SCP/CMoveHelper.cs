
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Dicom.Log;
using Dicom.Network;

namespace CMoveSCP
{

    /// <summary>
    /// classe permettant de g�rer le CMove
    /// </summary>
    public class CMoveHelper
    {

        #region private classes

        /// <summary>
        /// r�sultat du send
        /// </summary>
        private class SendResult
        {
            public int Completed { get; private set; }
            public int Remaining { get; private set; }
            public int Failures { get; private set; }
            public int Warnings { get; private set; }

            public SendResult(int completed, int remaining, int failures, int warnings)
            {
                Completed = completed;
                Remaining = remaining;
                Failures = failures;
                Warnings = warnings;
            }
        }

        #endregion

        /// <summary>
        /// logger interne
        /// </summary>
        private readonly Logger _logger;

        /// <summary>
        /// title de l'ae qui g�re le CMove
        /// </summary>
        private readonly string _aeTitle;

        /// <summary>
        /// finder d'images pour le CMove
        /// </summary>
        private readonly ICMoveImageFinder _cMoveImageFinder;

        /// <summary>
        /// permet d'authoriser les Ae � recevoir le CMove
        /// </summary>
        private readonly IAeCMoveAuthorizer _iaeCMoveAuthorizer;

        /// <summary>
        /// constructeur de CMoveHelper
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="aeTitle">title de l'ae qui g�re le CMove </param>
        /// <param name="cMoveImageFinder"> finder d'images pour le CMove</param>
        /// <param name="iaeCMoveAuthorizer">permet d'authoriser les Ae � recevoir le CMove</param>
        public CMoveHelper(Logger logger,string aeTitle,ICMoveImageFinder cMoveImageFinder, IAeCMoveAuthorizer iaeCMoveAuthorizer)
        {
            _logger = logger;
            _aeTitle = aeTitle;
            _cMoveImageFinder = cMoveImageFinder;
            _iaeCMoveAuthorizer = iaeCMoveAuthorizer;
        }

        //autre version de l'implem ici
        //ou il envoit un DicomCMove apr�s chaque requ�te correctement eenvoy�e
        //https://groups.google.com/forum/#!topic/fo-dicom/ic35XVDD1Hc
        //c'est optionnel d'apr�s la spec

        /// <summary>
        /// handler d'une CMove Request
        /// </summary>
        /// <param name="cMoveRequest"></param>
        /// <returns></returns>
        public IEnumerable<DicomCMoveResponse> OnCMoveRequest(DicomCMoveRequest cMoveRequest)
        {
        
            bool isAeAllowedToReceiveCMove = false;
            ApplicationEntityNetworkInfos aeDestinationNetworkInfos=null;
            TryCatchLog(() =>
            {
                //on regarde si L'application entity est connue
                //si cen 'est pas le cas, le CMove n'est pas autoris�.
                //Si c'est bon, on r�cup�re ses infos r�seau pour pouvoir faire des CStore Request
                isAeAllowedToReceiveCMove = _iaeCMoveAuthorizer.IsAeAllowedToReceiveCMove(
                cMoveRequest.DestinationAE,
                null /* TODO adresse ip, en attente de patch fo dicom*/,
                out aeDestinationNetworkInfos);
            }, "IsAeAllowedToReceiveCMove");
            
            if (!isAeAllowedToReceiveCMove)
            {
                //la destination n'est pas accept�e on renvoit une erreur
                yield return new DicomCMoveResponse(cMoveRequest, DicomStatus.QueryRetrieveMoveDestinationUnknown);
                yield break;
            }

         
            string[] imagesFilePathsToSend;
            SendResult sendRes=null;
        
            bool noError=TryCatchLog(() =>
            {
                //on r�cup�re les chemins des images � envoyer
                imagesFilePathsToSend = _cMoveImageFinder.GetImagesFilePathsToSend(cMoveRequest);

                //envoi des images
                sendRes = SendImages(cMoveRequest, aeDestinationNetworkInfos, imagesFilePathsToSend);
            }, "Send Images");

            //http://dicom.nema.org/medical/dicom/current/output/chtml/part04/sect_C.4.2.html
            //TODO il faudrait g�rer d'apr�s la spec chaque warning, failures

            //en cas d'erreur on envoit une r�ponse d'erreur
            if (!noError)
            {
                yield return new DicomCMoveResponse(cMoveRequest, DicomStatus.QueryRetrieveUnableToProcess);
                yield break;
            }

            //Cmove termin�, on l'indique � celui qui a initi� la requ�te CMove
            yield return new DicomCMoveResponse(cMoveRequest, DicomStatus.Success)
            {
                Completed = sendRes.Completed,
                Remaining = sendRes.Remaining,
                Failures = sendRes.Failures,
                Warnings = sendRes.Warnings             
            };
        }

        /// <summary>
        /// envoi les images
        /// </summary>
        /// <param name="cMoveRequest"></param>
        /// <param name="aeDestinationNetworkInfos"></param>
        /// <param name="imagesToSend"></param>
        /// <returns></returns>
        private SendResult SendImages(DicomCMoveRequest cMoveRequest, ApplicationEntityNetworkInfos aeDestinationNetworkInfos, IEnumerable<string> imagesToSend)
        {
            DicomClient client = new DicomClient();

            //envoi des images
            foreach (string imagePath in imagesToSend)
            {
                client.AddRequest(new DicomCStoreRequest(imagePath));
            }

            client.Send(aeDestinationNetworkInfos.HostNameOrIp, aeDestinationNetworkInfos.Port,
                false, _aeTitle, cMoveRequest.DestinationAE);

            return new SendResult(imagesToSend.Count(),0,0,0);
        }


        /// <summary>
        /// helper pour faire un try catch simplifi� avec logging d'une exception
        /// </summary>
        /// <param name="action">action � executer dans le try</param>
        /// <param name="memberName"></param>
        /// <returns>true s'il n'y a pas eu d'exception, false sinon</returns>
        private bool TryCatchLog(Action action, [CallerMemberName]string memberName = "")
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("exception survenue dans {0} : {1}", memberName, ex.ToString()));
                return false;
            }
        }


     
    }
}