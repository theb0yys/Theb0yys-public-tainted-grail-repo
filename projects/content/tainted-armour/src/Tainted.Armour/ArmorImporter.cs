using System;
using System.Collections.Generic;
using System.Threading;

namespace Tainted.Armour;

public sealed class ArmorImporter
{
    private readonly DeformationValidationStage deformationStage = new DeformationValidationStage();
    private readonly VisualRuntimeObservationStage visualRuntimeStage = new VisualRuntimeObservationStage();

    public ArmorImportResult Import(ArmorImportRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        cancellationToken.ThrowIfCancellationRequested();

        DeformationValidationResult deformation = deformationStage.Execute(
            request.Source,
            request.Target,
            request.Deformation,
            cancellationToken);
        VisualRuntimeValidationResult visualRuntime = visualRuntimeStage.Execute(
            request.VisualRuntimeObservation,
            cancellationToken);

        var blockers = new List<string>(deformation.Blockers);
        blockers.AddRange(visualRuntime.Blockers);
        blockers.Add("production_conversion_blocked:converter_serializer_metadata_stream_archive_alias_runtime_registration_evidence_missing");

        ArmorImportStatus status;
        if (!deformation.DeformationAccepted)
        {
            status = ArmorImportStatus.DeformationBlocked;
        }
        else if (visualRuntime.Requested && !visualRuntime.VisualRuntimeAccepted)
        {
            status = ArmorImportStatus.VisualRuntimeBlocked;
        }
        else if (visualRuntime.Requested)
        {
            status = ArmorImportStatus.DeformationAndVisualRuntimeValidatedConversionBlocked;
        }
        else
        {
            status = ArmorImportStatus.DeformationValidatedConversionBlocked;
        }

        return new ArmorImportResult(request.RequestId, status, deformation, visualRuntime, blockers);
    }
}
