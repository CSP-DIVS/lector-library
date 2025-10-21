SELECT 
    SUM(CASE WHEN Status = 'Outstanding' THEN Amount ELSE 0 END) AS TotalOutstanding,
    SUM(CASE WHEN Status = 'Outstanding' THEN 1 ELSE 0 END) AS OutstandingCount,
    SUM(CASE WHEN Status = 'Paid' THEN Amount ELSE 0 END) AS TotalPaid,
    SUM(CASE WHEN Status = 'Paid' THEN 1 ELSE 0 END) AS PaidCount,
    SUM(CASE WHEN Status = 'Waived' THEN Amount ELSE 0 END) AS TotalWaived,
    SUM(CASE WHEN Status = 'Waived' THEN 1 ELSE 0 END) AS WaivedCount
FROM Fines
WHERE UserId = @UserId;
