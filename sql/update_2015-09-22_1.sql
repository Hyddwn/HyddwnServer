CREATE TABLE IF NOT EXISTS `friends` (
  `friendId` bigint(20) NOT NULL AUTO_INCREMENT,
  `userId1` int(11) NOT NULL,
  `userId2` int(11) NOT NULL,
  `groupId` int(11) NOT NULL,
  `status` tinyint(4) NOT NULL,
  PRIMARY KEY (`friendId`)
) ENGINE=InnoDB  DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;
