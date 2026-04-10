using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UTools;

public class RemoteConfigService : IAsyncInitializable
{

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Delay(3000, cancellationToken);

        return;
    }


}
