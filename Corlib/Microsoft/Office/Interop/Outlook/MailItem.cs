using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;

namespace Microsoft.Office.Interop.Outlook {

  internal static partial class MailItemExtensions {

    public static void AddMailToRecipients(this MailItem @this, string[] mailTo) {
      if (mailTo == null || mailTo.Length == 0)
        throw new ArgumentNullException(nameof(mailTo));

      var recipients = @this.Recipients;
      foreach (var address in mailTo.Where(a => !string.IsNullOrWhiteSpace(a)))
        recipients.Add(address);

      recipients.ResolveAll();
    }

    public static void AddAttachments(this MailItem @this, FileInfo[] filesToAttach) {
      Contract.Requires(@this != null && filesToAttach != null && filesToAttach.Length > 0);
      var attachments = @this.Attachments;
      foreach (var file in filesToAttach)
        attachments.Add(file.FullName, OlAttachmentType.olByValue, 1, file.Name);

    }
  }
}