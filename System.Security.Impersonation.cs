using System.DirectoryServices.ActiveDirectory;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Permissions;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;

namespace System.Security {

  [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
  public class Impersonation : IDisposable {

    #region nested types

    public enum LogonType {
      /// <summary>
      /// This logon type is intended for users who will be interactively using the computer, such as a user being logged on  
      /// by a terminal server, remote shell, or similar process.
      /// This logon type has the additional expense of caching logon information for disconnected operations; 
      /// therefore, it is inappropriate for some client/server applications,
      /// such as a mail server.
      /// </summary>
      Interactive = 2,

      /// <summary>
      /// This logon type is intended for high performance servers to authenticate plaintext passwords.

      /// The LogonUser function does not cache credentials for this logon type.
      /// </summary>
      Network = 3,

      /// <summary>
      /// This logon type is intended for batch servers, where processes may be executing on behalf of a user without 
      /// their direct intervention. This type is also for higher performance servers that process many plaintext
      /// authentication attempts at a time, such as mail or Web servers. 
      /// The LogonUser function does not cache credentials for this logon type.
      /// </summary>
      Batch = 4,

      /// <summary>
      /// Indicates a service-type logon. The account provided must have the service privilege enabled. 
      /// </summary>
      Service = 5,

      /// <summary>
      /// This logon type is for GINA DLLs that log on users who will be interactively using the computer. 
      /// This logon type can generate a unique audit record that shows when the workstation was unlocked. 
      /// </summary>
      Unlock = 7,

      /// <summary>
      /// This logon type preserves the name and password in the authentication package, which allows the server to make 
      /// connections to other network servers while impersonating the client. A server can accept plaintext credentials 
      /// from a client, call LogonUser, verify that the user can access the system across the network, and still 
      /// communicate with other servers.
      /// NOTE: Windows NT:  This value is not supported. 
      /// </summary>
      NetworkCleartext = 8,

      /// <summary>
      /// This logon type allows the caller to clone its current token and specify new credentials for outbound connections.
      /// The new logon session has the same local identifier but uses different credentials for other network connections. 
      /// NOTE: This logon type is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.
      /// NOTE: Windows NT:  This value is not supported. 
      /// </summary>
      NewCredentials = 9,
    }

    public enum LogonProvider {
      /// <summary>
      /// Use the standard logon provider for the system. 
      /// The default security provider is negotiate, unless you pass NULL for the domain name and the user name 
      /// is not in UPN format. In this case, the default provider is NTLM. 
      /// NOTE: Windows 2000/NT:   The default security provider is NTLM.
      /// </summary>
      Default = 0,
      WinNT35 = 1,
      WinNT40_NTLM = 2,
      WinNT50 = 3
    }

    private sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid {
      private SafeTokenHandle() : base(true) { }

      [DllImport("kernel32.dll")]
      [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
      [SuppressUnmanagedCodeSecurity]
      [return: MarshalAs(UnmanagedType.Bool)]
      private static extern bool CloseHandle(IntPtr handle);

      protected override bool ReleaseHandle() => CloseHandle(this.handle);
    }

    #endregion

    private const int _UNKNOWN_USERNAME_OR_BAD_PASSWORD = 1326;

    private readonly SafeTokenHandle _handle;
    private readonly WindowsImpersonationContext _context;
    private bool _isDisposed;

    public Impersonation(string username, string password, LogonType type = LogonType.Interactive, LogonProvider provider = LogonProvider.Default) : this(Domain.GetCurrentDomain().Name, username, password, type, provider) { }

    public Impersonation(string domain, string username, string password, LogonType type = LogonType.Interactive, LogonProvider provider = LogonProvider.Default) {
      if (!LogonUser(username, domain, password, (int)type, (int)provider, out this._handle)) {
        var error = Marshal.GetLastWin32Error();
        if (error == _UNKNOWN_USERNAME_OR_BAD_PASSWORD)
          throw new InvalidCredentialException("Could not impersonate the elevated user. LogonUser returned: unknown username or bad password.");
        throw new ApplicationException($"Could not impersonate the elevated user. LogonUser returned: error code {error}.");
      }

      this._context = WindowsIdentity.Impersonate(this._handle.DangerousGetHandle());
    }

    ~Impersonation() {
      this.Dispose();
    }

    public void Dispose() {
      if (this._isDisposed)
        return;

      this._isDisposed = true;

      this._context?.Dispose();
      if (!this._handle.IsInvalid)
        this._handle.Dispose();

      GC.SuppressFinalize(this);
    }

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);
  }
}
