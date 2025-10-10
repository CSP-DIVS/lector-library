SELECT Id, Username, Email, PasswordHash, Role, IsActive 
FROM users 
WHERE Username = @Identifier OR Email = @Identifier