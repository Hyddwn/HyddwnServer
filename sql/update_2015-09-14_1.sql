CREATE TABLE IF NOT EXISTS `notes` (
  `noteId` int(11) NOT NULL AUTO_INCREMENT,
  `sender` varchar(127) NOT NULL,
  `receiver` varchar(127) NOT NULL,
  `message` varchar(255) NOT NULL,
  `time` datetime NOT NULL,
  `read` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`noteId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8 AUTO_INCREMENT=1 ;
