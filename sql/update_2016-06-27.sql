ALTER TABLE `items` ADD `bankTransferStart` DATETIME NULL DEFAULT NULL AFTER `bank`, ADD `bankTransferDuration` INT NOT NULL DEFAULT '0' AFTER `bankTransferStart`;
