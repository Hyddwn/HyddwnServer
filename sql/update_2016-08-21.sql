ALTER TABLE `guilds` ADD `visibility` INT NOT NULL DEFAULT '0' AFTER `type`;
ALTER TABLE `guilds` CHANGE `title` `title` VARCHAR(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL DEFAULT '';
ALTER TABLE `guild_members` CHANGE `application` `application` VARCHAR(255) CHARACTER SET utf8 COLLATE utf8_general_ci NOT NULL DEFAULT '';
ALTER TABLE `guild_members` CHANGE `rank` `rank` INT(11) NOT NULL DEFAULT '5';
ALTER TABLE `guild_members` CHANGE `messages` `messages` INT(11) NOT NULL DEFAULT '0';
