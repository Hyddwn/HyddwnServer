UPDATE `creatures` SET `lastRebirth` = `lastAging` WHERE `lastRebirth` IS NULL;
ALTER TABLE `creatures` CHANGE `lastRebirth` `lastRebirth` DATETIME NOT NULL;
