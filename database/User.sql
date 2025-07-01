use Otomobil_DB;
ALTER TABLE users
  ADD COLUMN isEmailVerified BOOLEAN DEFAULT FALSE AFTER isActive,
  ADD COLUMN emailVerificationToken VARCHAR(255) NULL AFTER isEmailVerified,
  ADD COLUMN emailTokenCreatedAt DATETIME NULL AFTER emailVerificationToken,
  ADD COLUMN passwordResetToken VARCHAR(255) NULL AFTER emailTokenCreatedAt,
  ADD COLUMN passwordResetTokenCreatedAt DATETIME NULL AFTER passwordResetToken;
