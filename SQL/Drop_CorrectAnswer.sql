-- Run this script to drop the CorrectAnswer column from your live database.
-- Note: This is an irreversible operation. It relies on OptionA containing the correct answer moving forward.

ALTER TABLE FP_Questions2026
DROP COLUMN CorrectAnswer;
GO
