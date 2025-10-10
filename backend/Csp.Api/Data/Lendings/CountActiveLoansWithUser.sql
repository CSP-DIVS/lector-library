SELECT COUNT(*) FROM lendings l
WHERE l.Status IN ('Active', 'Overdue') AND l.UserId = @UserId