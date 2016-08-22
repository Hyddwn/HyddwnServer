CREATE TABLE IF NOT EXISTS `guilds` (
  `guildId` bigint(20) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL,
  `leaderName` varchar(255) NOT NULL,
  `title` varchar(255) NOT NULL,
  `introMessage` varchar(255) NOT NULL,
  `welcomeMessage` varchar(255) NOT NULL,
  `leavingMessage` varchar(255) NOT NULL,
  `rejectionMessage` varchar(255) NOT NULL,
  `type` int(11) NOT NULL,
  `level` int(11) NOT NULL,
  `options` int(11) NOT NULL,
  `stonePropId` int(11) NOT NULL,
  `stoneRegionId` int(11) NOT NULL,
  `stoneX` int(11) NOT NULL,
  `stoneY` int(11) NOT NULL,
  `stoneDirection` float NOT NULL,
  `points` int(11) NOT NULL,
  `gold` int(11) NOT NULL,
  PRIMARY KEY (`guildId`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=216172782119026689 ;

INSERT INTO `guilds` (`guildId`, `name`, `leaderName`, `title`, `introMessage`, `welcomeMessage`, `leavingMessage`, `rejectionMessage`, `type`, `level`, `options`, `stonePropId`, `stoneRegionId`, `stoneX`, `stoneY`, `stoneDirection`, `points`, `gold`) VALUES
(216172782119026688, '_Dummy', 'Aura', '', '', '', '', '', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

CREATE TABLE IF NOT EXISTS `guild_members` (
  `guildId` bigint(20) NOT NULL,
  `characterId` bigint(20) NOT NULL,
  `rank` int(11) NOT NULL,
  `joinedDate` datetime NOT NULL,
  `points` int(11) NOT NULL,
  `application` varchar(255) NOT NULL,
  `messages` int(11) NOT NULL,
  PRIMARY KEY (`guildId`,`characterId`),
  KEY `characterId` (`characterId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


ALTER TABLE `guild_members`
  ADD CONSTRAINT `guild_members_ibfk_2` FOREIGN KEY (`characterId`) REFERENCES `characters` (`entityId`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `guild_members_ibfk_1` FOREIGN KEY (`guildId`) REFERENCES `guilds` (`guildId`) ON DELETE CASCADE ON UPDATE CASCADE;
