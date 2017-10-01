ALTER TABLE `creatures` ADD `lastDeSpawn` DATETIME NULL DEFAULT NULL AFTER `playTime`, ADD `remainingTime` TIME NULL DEFAULT NULL AFTER `lastDeSpawn`;
