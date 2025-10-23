SELECT 
    p.Id,
    p.FineId,
    p.UserId,
    u.Username AS MemberName,
    u.Email AS MemberEmail,
    p.Amount,
    p.TransactionId,
    p.Description,
    p.PaymentDate
FROM Payments p
INNER JOIN Fines f ON p.FineId = f.Id
INNER JOIN Users u ON f.UserId = u.Id
ORDER BY p.PaymentDate DESC
LIMIT @PageSize OFFSET @Offset;
