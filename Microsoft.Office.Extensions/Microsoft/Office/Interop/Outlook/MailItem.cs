#region (c)2010-2042 Hawkynt

// This file is part of Hawkynt's .NET Framework extensions.
// 
// Hawkynt's .NET Framework extensions are free software:
// you can redistribute and/or modify it under the terms
// given in the LICENSE file.
// 
// Hawkynt's .NET Framework extensions is distributed in the hope that
// it will be useful, but WITHOUT ANY WARRANTY without even the implied
// warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the LICENSE file for more details.
// 
// You should have received a copy of the License along with Hawkynt's
// .NET Framework extensions. If not, see
// <https://github.com/Hawkynt/C--FrameworkExtensions/blob/master/LICENSE>.

#endregion

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
