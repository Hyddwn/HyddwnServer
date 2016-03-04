ALTER TABLE `creatures` ADD `defense` SMALLINT(6) NOT NULL DEFAULT '0' AFTER `luckBonus`, ADD `protection` FLOAT NOT NULL DEFAULT '0' AFTER `defense`;
