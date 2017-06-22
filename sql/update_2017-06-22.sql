CREATE TABLE `cool_downs` (
  `coolDownId` bigint(20) NOT NULL,
  `creatureId` bigint(20) NOT NULL,
  `identifier` varchar(128) NOT NULL,
  `end` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

ALTER TABLE `cool_downs`
  ADD PRIMARY KEY (`coolDownId`),
  ADD KEY `creatureId` (`creatureId`);

ALTER TABLE `cool_downs`
  MODIFY `coolDownId` bigint(20) NOT NULL AUTO_INCREMENT;

ALTER TABLE `cool_downs`
  ADD CONSTRAINT `cool_downs_ibfk_1` FOREIGN KEY (`creatureId`) REFERENCES `creatures` (`creatureId`) ON DELETE CASCADE ON UPDATE CASCADE;
