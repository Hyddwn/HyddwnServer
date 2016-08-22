ALTER TABLE `guilds` ADD `establishedDate` DATETIME NOT NULL AFTER `title`;
UPDATE `guilds` SET `establishedDate` = '2016-08-19 00:00:00' WHERE `guilds`.`guildId` = 216172782119026688;

ALTER TABLE `guilds` ADD `server` VARCHAR(50) NOT NULL AFTER `establishedDate`;
UPDATE `guilds` SET `server` = 'Aura' WHERE `guilds`.`guildId` = 216172782119026688;
