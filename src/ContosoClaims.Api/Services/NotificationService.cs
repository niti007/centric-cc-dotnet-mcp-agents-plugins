using ContosoClaims.Api.Models;

namespace ContosoClaims.Api.Services;

public class NotificationService
{
    public string BuildClaimUpdateEmail(Claim claim, ClaimNote? latestNote, string holderName)
    {
        var noteSection = latestNote is null
            ? string.Empty
            : $"<p><strong>Latest note:</strong> {latestNote.Body}</p>";

        var body = $@"
<html>
<body>
<p>Dear {holderName},</p>
<p>Your claim <strong>{claim.ClaimNumber}</strong> has been updated to status <strong>{claim.Status}</strong>.</p>
<p><strong>Description on file:</strong> {claim.Description}</p>
{noteSection}
<p>Thank you for choosing Contoso Insurance.</p>
</body>
</html>";

        return body;
    }
}
