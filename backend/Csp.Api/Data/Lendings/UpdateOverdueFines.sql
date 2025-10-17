-- Updates fine amount on Lending records which are overdue and not returned
-- Idempotent with respect to the current date and rate
-- We keep fines cumulative = days_overdue * rate

UPDATE lendings l
SET l.FineAmount = GREATEST(DATEDIFF(@Today, l.DueDate), 0) * @DailyRate,
    l.Status = CASE WHEN DATEDIFF(@Today, l.DueDate) > 0 AND l.ReturnDate IS NULL THEN 'Overdue' ELSE l.Status END,
    l.UpdatedAt = UTC_TIMESTAMP()
WHERE l.ReturnDate IS NULL
  AND l.DueDate < @Today;