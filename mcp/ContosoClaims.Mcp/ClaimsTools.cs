using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;
using MySqlConnector;

namespace ContosoClaims.Mcp;

/// <summary>
/// MCP tools exposing read access to the Contoso Claims database (policies, adjusters, claims,
/// claim_notes, payments). Connection string is read from the CONTOSO_CLAIMS_CONNECTION
/// environment variable, falling back to the local workshop MySQL instance on port 3307.
/// </summary>
[McpServerToolType]
public static class ClaimsTools
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("CONTOSO_CLAIMS_CONNECTION")
        ?? "Server=127.0.0.1;Port=3307;User ID=root;Password=ContosoDemo!23;Database=contoso_claims";

    private static MySqlConnection OpenConnection()
    {
        var connection = new MySqlConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    [McpServerTool(Name = "list_open_claims")]
    [Description(
        "Lists claims that are still open (status 'submitted' or 'under_review'), most recently " +
        "reported first. Returns claim_number, status, claimed_amount, incident_date, and the full " +
        "name of the currently assigned adjuster (or null if unassigned). Use this to see the current " +
        "open workload, optionally narrowed to a single adjuster. Optional 'adjusterId' filters to " +
        "claims whose assigned_adjuster_id matches; optional 'limit' caps the number of rows returned " +
        "(default 20, maximum 100).")]
    public static async Task<string> ListOpenClaims(
        [Description("Only return claims assigned to this adjuster's numeric id. Omit to see open claims for all adjusters.")]
        int? adjusterId = null,
        [Description("Maximum number of rows to return. Default 20, hard capped at 100.")]
        int? limit = null)
    {
        var effectiveLimit = Math.Clamp(limit ?? 20, 1, 100);

        await using var connection = OpenConnection();
        await using var command = connection.CreateCommand();

        var sql = new StringBuilder(@"
SELECT
    c.claim_number,
    c.status,
    c.claimed_amount,
    c.incident_date,
    a.full_name AS assigned_adjuster_name
FROM claims c
LEFT JOIN adjusters a ON a.id = c.assigned_adjuster_id
WHERE c.status IN ('submitted', 'under_review')");

        if (adjusterId.HasValue)
        {
            sql.Append(" AND c.assigned_adjuster_id = @adjusterId");
            command.Parameters.AddWithValue("@adjusterId", adjusterId.Value);
        }

        sql.Append(" ORDER BY c.reported_at DESC LIMIT @limit");
        command.Parameters.AddWithValue("@limit", effectiveLimit);
        command.CommandText = sql.ToString();

        var rows = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var claimNumber = reader.GetString("claim_number");
            var status = reader.GetString("status");
            var claimedAmount = reader.GetDecimal("claimed_amount");
            var incidentDate = reader.GetDateTime("incident_date");
            var adjusterName = reader.IsDBNull(reader.GetOrdinal("assigned_adjuster_name"))
                ? "(unassigned)"
                : reader.GetString("assigned_adjuster_name");

            rows.Add(
                $"{claimNumber} | status={status} | claimed_amount={claimedAmount:F2} | " +
                $"incident_date={incidentDate:yyyy-MM-dd} | assigned_adjuster={adjusterName}");
        }

        if (rows.Count == 0)
        {
            return "No open claims found for the given filters.";
        }

        return string.Join("\n", rows);
    }

    [McpServerTool(Name = "get_claim")]
    [Description(
        "Fetches the full detail for a single claim by its claim number (e.g. 'CLM-2025-00317'): " +
        "status, dates, claimed/approved amounts, the policy's holder_name and policy_number, the " +
        "full name of the assigned adjuster, the full name of the deciding adjuster (if any decision " +
        "has been made), and all claim_notes attached to it, oldest first. Use this when you need the " +
        "complete picture of one specific claim rather than a list. Returns a clear not-found message " +
        "if the claim number does not exist, rather than raising an error.")]
    public static async Task<string> GetClaim(
        [Description("The claim's unique claim_number, e.g. 'CLM-2025-00317'. Case-sensitive exact match.")]
        string claimNumber)
    {
        await using var connection = OpenConnection();

        await using var claimCommand = connection.CreateCommand();
        claimCommand.CommandText = @"
SELECT
    c.claim_number,
    c.status,
    c.incident_date,
    c.reported_at,
    c.description,
    c.claimed_amount,
    c.approved_amount,
    c.decided_at,
    p.holder_name,
    p.policy_number,
    assigned.full_name AS assigned_adjuster_name,
    decided.full_name AS decided_by_adjuster_name
FROM claims c
JOIN policies p ON p.id = c.policy_id
LEFT JOIN adjusters assigned ON assigned.id = c.assigned_adjuster_id
LEFT JOIN adjusters decided ON decided.id = c.decided_by_adjuster_id
WHERE c.claim_number = @claimNumber";
        claimCommand.Parameters.AddWithValue("@claimNumber", claimNumber);

        string status, description, holderName, policyNumber;
        DateTime incidentDate, reportedAt;
        decimal claimedAmount;
        decimal? approvedAmount;
        DateTime? decidedAt;
        string assignedAdjusterName, decidedByAdjusterName;

        await using (var reader = await claimCommand.ExecuteReaderAsync())
        {
            if (!await reader.ReadAsync())
            {
                return $"No claim found with claim number '{claimNumber}'.";
            }

            status = reader.GetString("status");
            incidentDate = reader.GetDateTime("incident_date");
            reportedAt = reader.GetDateTime("reported_at");
            description = reader.GetString("description");
            claimedAmount = reader.GetDecimal("claimed_amount");
            approvedAmount = reader.IsDBNull(reader.GetOrdinal("approved_amount"))
                ? null
                : reader.GetDecimal("approved_amount");
            decidedAt = reader.IsDBNull(reader.GetOrdinal("decided_at"))
                ? null
                : reader.GetDateTime("decided_at");
            holderName = reader.GetString("holder_name");
            policyNumber = reader.GetString("policy_number");
            assignedAdjusterName = reader.IsDBNull(reader.GetOrdinal("assigned_adjuster_name"))
                ? "(unassigned)"
                : reader.GetString("assigned_adjuster_name");
            decidedByAdjusterName = reader.IsDBNull(reader.GetOrdinal("decided_by_adjuster_name"))
                ? "(not yet decided)"
                : reader.GetString("decided_by_adjuster_name");
        }

        await using var notesCommand = connection.CreateCommand();
        notesCommand.CommandText = @"
SELECT n.created_at, n.body, a.full_name AS author_name
FROM claim_notes n
JOIN claims c ON c.id = n.claim_id
LEFT JOIN adjusters a ON a.id = n.author_adjuster_id
WHERE c.claim_number = @claimNumber
ORDER BY n.created_at ASC";
        notesCommand.Parameters.AddWithValue("@claimNumber", claimNumber);

        var notes = new List<string>();
        await using (var reader = await notesCommand.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var createdAt = reader.GetDateTime("created_at");
                var body = reader.GetString("body");
                var authorName = reader.IsDBNull(reader.GetOrdinal("author_name"))
                    ? "(unknown author)"
                    : reader.GetString("author_name");
                notes.Add($"  [{createdAt:yyyy-MM-dd HH:mm}] {authorName}: {body}");
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Claim {claimNumber}");
        sb.AppendLine($"  Policy: {policyNumber} (holder: {holderName})");
        sb.AppendLine($"  Status: {status}");
        sb.AppendLine($"  Incident date: {incidentDate:yyyy-MM-dd}");
        sb.AppendLine($"  Reported at: {reportedAt:yyyy-MM-dd HH:mm}");
        sb.AppendLine($"  Claimed amount: {claimedAmount:F2}");
        sb.AppendLine($"  Approved amount: {(approvedAmount.HasValue ? approvedAmount.Value.ToString("F2") : "(none)")}");
        sb.AppendLine($"  Assigned adjuster: {assignedAdjusterName}");
        sb.AppendLine($"  Decided by adjuster: {decidedByAdjusterName}");
        sb.AppendLine($"  Decided at: {(decidedAt.HasValue ? decidedAt.Value.ToString("yyyy-MM-dd HH:mm") : "(not yet decided)")}");
        sb.AppendLine($"  Description: {description}");
        sb.AppendLine(notes.Count == 0 ? "  Notes: (none)" : "  Notes:");
        foreach (var note in notes)
        {
            sb.AppendLine(note);
        }

        return sb.ToString();
    }

    // ------------------------------------------------------------------------------------------
    // EXERCISE STUB — learner's task.
    //
    // Build a query that groups claims by status and returns, per status, the claim count and the
    // sum of claimed_amount. Expected SQL shape:
    //
    //   SELECT status, COUNT(*) AS claim_count, SUM(claimed_amount) AS total_claimed
    //   FROM claims
    //   GROUP BY status
    //   ORDER BY status;
    //
    // Expected return shape: one line per status, e.g.
    //   "submitted | count=60 | total_claimed=123456.78"
    // for each of the five status values (submitted, under_review, approved, rejected, paid).
    // ------------------------------------------------------------------------------------------
    [McpServerTool(Name = "claim_stats_by_status")]
    [Description(
        "EXERCISE STUB (not implemented): should return, for each claim status, the number of " +
        "claims and the total claimed_amount in that status. Learner's task — see the comment above " +
        "this method in ClaimsTools.cs for the expected SQL and return shape.")]
    public static Task<string> ClaimStatsByStatus()
    {
        // Returned rather than thrown: an exception surfaces to the MCP client as a generic
        // "an error occurred", which tells the learner nothing. This way the instruction is
        // what they actually see when they call the tool.
        return Task.FromResult(
            "NOT IMPLEMENTED — this is your exercise.\n\n" +
            "Implement claim_stats_by_status in mcp/ContosoClaims.Mcp/ClaimsTools.cs: group claims " +
            "by status and return the count plus SUM(claimed_amount) for each status.\n" +
            "The comment directly above this method gives the expected SQL and the exact return " +
            "shape. Use a parameterised MySqlCommand, as the two working tools in this file do.");
    }

    // ------------------------------------------------------------------------------------------
    // EXERCISE STUB — learner's task.
    //
    // Look up an adjuster by full_name or employee_code (the caller may pass either), then return
    // every claim that adjuster *decided* (decided_by_adjuster_id), alongside the full_name of
    // whoever that same claim was *assigned* to (assigned_adjuster_id). This lets the caller spot
    // claims a person decided without ever being assigned to them.
    //
    // Expected SQL shape (parameterised, matching on full_name OR employee_code):
    //
    //   SELECT c.claim_number, c.status, c.claimed_amount,
    //          decided.full_name  AS decided_by_name,
    //          assigned.full_name AS assigned_to_name
    //   FROM claims c
    //   JOIN adjusters decided  ON decided.id = c.decided_by_adjuster_id
    //   LEFT JOIN adjusters assigned ON assigned.id = c.assigned_adjuster_id
    //   WHERE decided.full_name = @who OR decided.employee_code = @who
    //   ORDER BY c.decided_at DESC;
    //
    // Expected return shape: one line per claim, flagging any mismatch, e.g.
    //   "CLM-2025-00317 | status=approved | claimed_amount=4200.00 | decided_by=Jane Doe | assigned_to=John Smith | MISMATCH"
    //   "CLM-2025-00291 | status=paid     | claimed_amount=900.00  | decided_by=Jane Doe | assigned_to=Jane Doe   | ok"
    // ------------------------------------------------------------------------------------------
    [McpServerTool(Name = "find_claims_by_adjuster")]
    [Description(
        "EXERCISE STUB (not implemented): should look up an adjuster by full name or employee_code " +
        "and return every claim they decided, paired with who that claim was assigned to, so callers " +
        "can spot claims decided by someone other than the assigned adjuster. Learner's task — see " +
        "the comment above this method in ClaimsTools.cs for the expected SQL and return shape.")]
    public static Task<string> FindClaimsByAdjuster(
        [Description("The adjuster's full_name (e.g. 'Jane Doe') or employee_code (e.g. 'ADJ-004').")]
        string adjuster)
    {
        // Returned rather than thrown — see the note in ClaimStatsByStatus above.
        return Task.FromResult(
            $"NOT IMPLEMENTED — this is your exercise. (You passed adjuster: '{adjuster}')\n\n" +
            "Implement find_claims_by_adjuster in mcp/ContosoClaims.Mcp/ClaimsTools.cs: resolve the " +
            "given full_name or employee_code to an adjuster, then list every claim they decided " +
            "alongside who each claim was assigned to, flagging mismatches.\n" +
            "The comment directly above this method gives the expected SQL and the exact return " +
            "shape. Use a parameterised MySqlCommand, as the two working tools in this file do.");
    }

    // ------------------------------------------------------------------------------------------
    // SHIPPED WRITTEN, THEN DELIBERATELY DISABLED — teaching artefact.
    //
    // This is a fully working implementation of an arbitrary read-only SQL passthrough tool. It is
    // commented out on purpose and must stay that way; it exists so learners can see exactly what a
    // "just let the model write its own SELECT" tool looks like, and why it should not be exposed:
    //
    //   1. The only thing stopping the model from doing something you didn't intend is the English
    //      sentence in the tool's [Description] — there is no enforcement layer behind it. A model
    //      that misreads intent, or is steered by adversarial content in claim descriptions/notes
    //      (both free text, and the schema notes at least one contains raw HTML), can issue any
    //      SELECT it wants.
    //   2. "Read-only" is not something you can verify by inspecting the SQL string. Rejecting
    //      statements that don't start with SELECT does not stop stacked queries, comment tricks,
    //      information-disclosure via UNION, or a SELECT ... INTO OUTFILE, and MySqlConnector will
    //      happily execute whatever text you hand it in a single command.
    //   3. Even a genuinely read-only query can exfiltrate the entire database (all holder_email,
    //      full claim descriptions, adjuster emails) to whatever is consuming the tool's output —
    //      which, for an MCP tool, is an LLM context window, i.e. untrusted text going in and
    //      unbounded data going out.
    //   4. A handful of narrow, purpose-built tools (like list_open_claims and get_claim above) give
    //      you the same value with a reviewable, bounded surface area. That's the tradeoff this
    //      exercise is meant to make visible.
    // ------------------------------------------------------------------------------------------
    //
    // [McpServerTool(Name = "run_readonly_query")]
    // [Description(
    //     "Runs an arbitrary read-only SQL SELECT statement against the claims database and returns " +
    //     "the resulting rows. DO NOT ENABLE: kept for reference only, see the comment block above.")]
    // public static async Task<string> RunReadonlyQuery(
    //     [Description("A single SQL SELECT statement to execute. Non-SELECT statements are rejected.")]
    //     string sql)
    // {
    //     var trimmed = sql.TrimStart();
    //     if (!trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
    //     {
    //         return "Only SELECT statements are permitted.";
    //     }
    //
    //     await using var connection = OpenConnection();
    //     await using var command = connection.CreateCommand();
    //     command.CommandText = trimmed;
    //
    //     var rows = new List<string>();
    //     await using var reader = await command.ExecuteReaderAsync();
    //     while (await reader.ReadAsync())
    //     {
    //         var values = new object[reader.FieldCount];
    //         reader.GetValues(values);
    //         rows.Add(string.Join(", ", values.Select(v => v?.ToString() ?? "NULL")));
    //     }
    //
    //     return rows.Count == 0 ? "(no rows)" : string.Join("\n", rows);
    // }
}
