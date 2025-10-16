SELECT 
    p.Id AS PaymentId,
    p.MemberId,
    u.Username AS MemberName,
    u.Email AS MemberEmail,
    b.Title AS BookTitle,
    b.Author AS BookAuthor,
    p.Amount,
    p.PaymentDate,
    p.PaymentMethod,
    recorder.Username AS RecordedByName,
     CONCAT('TXN-', LPAD(p.Id, 8, '0')) AS TransactionId
FROM 
    payments p
    INNER JOIN lendings l ON p.LendingId = l.Id
    INNER JOIN users u ON p.MemberId = u.Id
    INNER JOIN books b ON l.BookId = b.Id
    LEFT JOIN users recorder ON p.RecordedBy = recorder.Id
WHERE 
    p.Id = @PaymentId;
