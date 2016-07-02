CREATE TABLE IF NOT EXISTS `quest_owls` (
  `creatureId` bigint(20) NOT NULL,
  `questId` int(11) NOT NULL,
  `arrival` datetime NOT NULL,
  KEY `creatureId` (`creatureId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

ALTER TABLE `quest_owls`
  ADD CONSTRAINT `quest_owls_ibfk_1` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE;
