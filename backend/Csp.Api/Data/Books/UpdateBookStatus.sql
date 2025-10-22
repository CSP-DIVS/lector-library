UPDATE books 
SET IsActive = @IsActive, UpdatedAt = CURRENT_TIMESTAMP
WHERE Id = @Id