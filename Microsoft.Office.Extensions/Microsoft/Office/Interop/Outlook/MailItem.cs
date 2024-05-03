#if !NETSTANDARD

using System.IO;
using System.Linq;
using Guard;

namespace Microsoft.Office.Interop.Outlook;

public static partial class MailItemExtensions {

  public static void AddMailToRecipients(this MailItem @this, params string[] mailTo) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(mailTo);
    
    var recipients = @this.Recipients;
    foreach (var address in mailTo.Where(a => !string.IsNullOrWhiteSpace(a)))
      recipients.Add(address);

    recipients.ResolveAll();
  }

  public static void AddAttachments(this MailItem @this, params FileInfo[] filesToAttach) {
    Against.ThisIsNull(@this);
    Against.ArgumentIsNullOrEmpty(filesToAttach);

    var attachments = @this.Attachments;
    foreach (var file in filesToAttach)
      attachments.Add(file.FullName, OlAttachmentType.olByValue, 1, file.Name);

  }
}

#endif