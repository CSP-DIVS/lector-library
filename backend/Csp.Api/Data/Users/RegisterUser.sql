INSERT INTO users (Username, Email, PasswordHash, Role, IsActive) 
VALUES (@Username, @Email, @PasswordHash, 'Member', 1);
SELECT LAST_INSERT_ID();