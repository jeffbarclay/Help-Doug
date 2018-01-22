<?php
error_reporting(E_ALL ^ E_DEPRECATED);
require("phpsqlinfo_dbinfo.php");

if(isset($_GET['station']) && !empty($_GET['station'])){
    $icon = $_GET['icon'];
	$station = $_GET['station'];
	//echo "<script type='text/javascript'>alert('aaa $icon bbb');</script>";
	$query = "SELECT * FROM hydrants WHERE `ICON` = '".$icon."' AND `STATION` = '".$station."';";
} else {   
	//echo "<script type='text/javascript'>alert('null queryString skip over to ALL STATIONS!');</script>";
	$query = "SELECT * FROM hydrants WHERE 1";
}

// Start XML file, create parent node
$dom = new DOMDocument("1.0");
$node = $dom->createElement("markers");
$parnode = $dom->appendChild($node);

// Opens a connection to a MySQL server
$connection=mysql_connect ('localhost', $username, $password);
if (!$connection) {  die('Not connected : ' . mysql_error());}

// Set the active MySQL database
$db_selected = mysql_select_db($database, $connection);
if (!$db_selected) {
  die ('Can\'t use db : ' . mysql_error());
}

$result = mysql_query($query);
//mysql_real_escape_string($query));
if (!$result) {
  die('Invalid query: ' . mysql_error());
}

header("Content-type: text/xml");

// Iterate through the rows, adding XML nodes for each
while ($row = @mysql_fetch_assoc($result)){
  // Add to XML document node
  $node = $dom->createElement("marker");
  $newnode = $parnode->appendChild($node);
  $newnode->setAttribute("st_num",$row['ST_NUM']);
  $newnode->setAttribute("st_name", $row['ST_NAME']);
  $newnode->setAttribute("lat", $row['lat']);
  $newnode->setAttribute("lng", $row['lng']);
  $newnode->setAttribute("station", $row['STATION']);
  $newnode->setAttribute("icon", $row['ICON']);
}

echo $dom->saveXML();
?>