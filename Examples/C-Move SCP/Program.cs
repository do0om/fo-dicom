// Copyright (c) 2012-2015 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using System;
using System.Collections.Generic;
using System.IO;
using CMoveSCP;
using Dicom.Log;
using Dicom.Network;

namespace Dicom.CMoveSCP
{
    internal class Program
    {
        private static string StoragePath = @".\DICOM";

        private static void Main(string[] args)
        {
            // preload dictionary to prevent timeouts
            var dict = DicomDictionary.Default;


            // start DICOM server on port 11112
            var server = new DicomServer<CMoveSCP>(11112);


            // end process
            Console.WriteLine("Press <return> to end...");
            Console.ReadLine();
        }

        private class CMoveSCP : DicomService, IDicomServiceProvider, IDicomCMoveProvider, IDicomCEchoProvider
        {
            private static DicomTransferSyntax[] AcceptedTransferSyntaxes = new DicomTransferSyntax[]
                                                                                {
                                                                                    DicomTransferSyntax
                                                                                        .ExplicitVRLittleEndian,
                                                                                    DicomTransferSyntax
                                                                                        .ExplicitVRBigEndian,
                                                                                    DicomTransferSyntax
                                                                                        .ImplicitVRLittleEndian
                                                                                };

            private static DicomTransferSyntax[] AcceptedImageTransferSyntaxes = new DicomTransferSyntax[]
                                                                                     {
                                                                                         // Lossless
                                                                                         DicomTransferSyntax
                                                                                             .JPEGLSLossless,
                                                                                         DicomTransferSyntax
                                                                                             .JPEG2000Lossless,
                                                                                         DicomTransferSyntax
                                                                                             .JPEGProcess14SV1,
                                                                                         DicomTransferSyntax
                                                                                             .JPEGProcess14,
                                                                                         DicomTransferSyntax
                                                                                             .RLELossless,

                                                                                         // Lossy
                                                                                         DicomTransferSyntax
                                                                                             .JPEGLSNearLossless,
                                                                                         DicomTransferSyntax
                                                                                             .JPEG2000Lossy,
                                                                                         DicomTransferSyntax
                                                                                             .JPEGProcess1,
                                                                                         DicomTransferSyntax
                                                                                             .JPEGProcess2_4,

                                                                                         // Uncompressed
                                                                                         DicomTransferSyntax
                                                                                             .ExplicitVRLittleEndian,
                                                                                         DicomTransferSyntax
                                                                                             .ExplicitVRBigEndian,
                                                                                         DicomTransferSyntax
                                                                                             .ImplicitVRLittleEndian
                                                                                     };

            public const string AeTitle = "MOVESCP";

            private readonly CMoveHelper _cMoveHelper;

            public CMoveSCP(Stream stream, Logger log)
                : base(stream, log)
            {
                _cMoveHelper = new CMoveHelper(log, AeTitle, new CMoveImageFinder(), aeCMoveAuthorizer);
            }

            public void OnReceiveAssociationRequest(DicomAssociation association)
            {
                if (association.CalledAE != AeTitle)
                {
                    SendAssociationReject(
                        DicomRejectResult.Permanent,
                        DicomRejectSource.ServiceUser,
                        DicomRejectReason.CalledAENotRecognized);
                    return;
                }

                foreach (var pc in association.PresentationContexts)
                {
                    //accept Echo
                    if (pc.AbstractSyntax == DicomUID.Verification) pc.AcceptTransferSyntaxes(AcceptedTransferSyntaxes);
                    //accept CMove
                    else if ((pc.AbstractSyntax == DicomUID.PatientRootQueryRetrieveInformationModelMOVE)
                             || (pc.AbstractSyntax == DicomUID.StudyRootQueryRetrieveInformationModelMOVE))
                        pc.AcceptTransferSyntaxes(AcceptedImageTransferSyntaxes);
                }

                SendAssociationAccept(association);
            }

            public void OnReceiveAssociationReleaseRequest()
            {
                SendAssociationReleaseResponse();
            }

            public void OnReceiveAbort(DicomAbortSource source, DicomAbortReason reason)
            {
            }

            public void OnConnectionClosed(Exception exception)
            {
            }

            public DicomCStoreResponse OnCStoreRequest(DicomCStoreRequest request)
            {
                var studyUid = request.Dataset.Get<string>(DicomTag.StudyInstanceUID);
                var instUid = request.SOPInstanceUID.UID;

                var path = Path.GetFullPath(Program.StoragePath);
                path = Path.Combine(path, studyUid);

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                path = Path.Combine(path, instUid) + ".dcm";

                request.File.Save(path);

                return new DicomCStoreResponse(request, DicomStatus.Success);
            }


            public DicomCEchoResponse OnCEchoRequest(DicomCEchoRequest request)
            {
                return new DicomCEchoResponse(request, DicomStatus.Success);
            }


            public IEnumerable<DicomCMoveResponse> OnCMoveRequest(DicomCMoveRequest request)
            {
                IEnumerable<DicomCMoveResponse> dicomCMoveResponses = _cMoveHelper.OnCMoveRequest(request);
                return dicomCMoveResponses;
            }
        }
    }
}
