using System.Text;
using Modules.Backup.Core.Entities.DbContext;
using Renci.SshNet;

namespace Modules.Backup.Core.StaticClasses;

public static class SshTunnelHelper
{
    public static IDisposable? OpenTunnel(DbServerConnection serverConnection, DbServerTunnel tunnelConfig)
    {
        if (!serverConnection.IsTunnelRequired) return null;

        SshClient sshClient;
        if (tunnelConfig.UsePasswordAuth)
        {
            sshClient = new SshClient(
                tunnelConfig.ServerHost,
                tunnelConfig.SshPort,
                tunnelConfig.Username,
                tunnelConfig.Password!);
        }
        else
        {
            using var keyStream = new MemoryStream(Encoding.UTF8.GetBytes(tunnelConfig.PrivateKeyContent!));
            var keyFile = new PrivateKeyFile(keyStream, tunnelConfig.PrivateKeyPassphrase);
            sshClient = new SshClient(
                tunnelConfig.ServerHost,
                tunnelConfig.SshPort,
                tunnelConfig.Username,
                keyFile);
        }

        sshClient.Connect();

        var portForward = new ForwardedPortLocal("127.0.0.1", (uint)tunnelConfig.LocalPort, tunnelConfig.RemoteHost,
            (uint)tunnelConfig.RemotePort);
        sshClient.AddForwardedPort(portForward);
        portForward.Start();

        // Zwracamy IDisposable, żeby w "using" zamknąć tunel automatycznie
        return new DisposableAction(() =>
        {
            portForward.Stop();
            sshClient.Disconnect();
            sshClient.Dispose();
        });
    }

    private class DisposableAction(Action action) : IDisposable
    {
        public void Dispose()
        {
            action();
        }
    }
}