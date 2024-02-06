
function toMonthName(monthNumber) {
    const date = new Date();
    date.setMonth(monthNumber - 1);
  
    return date.toLocaleString('en-US', {
      month: 'long',
    });
  }

var app = new Vue({
    el: '#app',
    delimiters: ['[[', ']]'],
    vuetify : new Vuetify({
        theme: { dark: true },
    }),
    data : {
        players : [],
        items: [],
        item_to_gift: "",
        bike_types : [{"name" : "Enduro", "val" : 0},  {"name" : "Hardtail", "val" : 2}, {"name" : "Downhill", "val" : 1}],
        commands: [
            {"evals" : ["RESPAWN_AT_START"], "name": "Respawn at Start", "risk": "high"},
            {"evals": ["RESPAWN_ON_TRACK"], "name": "Respawn on Track", "risk": "high"},
            {"evals": ["CUT_BRAKES"], "name": "Cut Brakes", "risk": "medium"},
            {"evals": ["BANNED|CRASH"], "name": "Crash Game", "risk": "extreme"},
            {"evals": ["BANNED|CLOSE"], "name": "Close Game", "risk": "extreme"},
            {"evals": ["TOGGLE_SPECTATOR"], "name": "Make 'em dissapear", "risk": "high"},
            {"evals": ["CLEAR_SESSION_MARKER"], "name": "Clear Session Marker", "risk": "low"},
            {"evals": ["RESET_PLAYER"],  "name": "Reset Player", "risk": "high"},
            {"evals": ["TOGGLE_COLLISION"], "name": "Toggle Collision", "risk": "low"},
            {"evals": ["SET_VEL|500"], "name": "Into Orbit", "risk": "high"},
            {"evals": ["ENABLE_STATS"], "name": "Enable Stats", "risk": "low"},
            {"evals": ["TOGGLE_GOD"], "name": "Activate Anticheat", "risk": "extreme"},
            {"evals": ["BAIL"], "name": "Kill.", "risk": "high"},
            {"evals": ["SET_BIKE_SIZE|0.5"], "name": "half player&bike size", "risk": "high"},
            {"evals": ["SET_BIKE_SIZE|2"], "name": "double player&bike size", "risk": "high"},
            {"evals": ["RIDERSGATE|0.5"], "name": "Trigger BMX Gate", "risk": "low"},
            {"evals": ["RIDERSGATE|0.5"], "name": "Trigger BMX Gate", "risk": "low"},
            {"evals": ["SET_FAR_CLIP|0.5"], "name": "bind 'em", "risk": "low"},
            {"evals": ["SET_FAR_CLIP|500"], "name": "carrot 'em", "risk": "low"},
            {"evals": ["INVALIDATE_TIME"], "name": "invalidate time", "risk": "low"},
            {"evals": ["SET_VEL|0"], "name": "stop 'em", "risk": "low"},
            {"evals": ["ROTATE|180"], "name": "Rotate 180", "risk": "low"},
            {"evals": ["ROTATE|180", "SET_VEL|-1"], "name": "Rotate and Push", "risk": "low"}
        ],
        controlled_player : null,
        validated: "UNAUTHORISED",
        display_pfps: false,
        times: [],
        manual_eval: "",
        trails: [],
        self: {}, // the object of the player currently logged in
        tab: 2, // the current tab selected
        search: null, // the current search being used to filter players
        cached_output_log: "", // the current output log being viewed
    },
    methods: {
        RunCommand(player, command){
            if (command.risk == "extreme"){
                confirmed = confirm("the command '"+command.name+"' has a risk rating of extreme - are you sure you want to run this?");
                if (!confirmed)
                    return;
            }
            if (player == "ALL"){
                this.players.forEach(id => {
                    if (id.id != "ALL")
                        command.evals.forEach(function(command_eval){
                            this.RunCommand(player.id, command_eval);
                        });
                });
            }
            else{
                command.evals.forEach(function(command_eval){
                    $.ajax({
                        url: "/eval/" + player.id + "?order=" + command_eval,
                        type: "GET",
                        success: function(data){},
                        error: function(data){
                            alert("RunCommand(); failed - are you sure you're logged in?");
                        }
                    });
                });
            }
        },
        TimeStateToColour(time){
            if (time.ignore == "True")
                return "red"
            if (time.verified == "0")
                return "yellow"
            return ""
        },
        SilentUrlSwitch(url){
            const nextURL = url;
            const nextTitle = 'Descenders Modkit';
            const nextState = { additionalInformation: 'Updated the URL with JS' };

            // This will create a new entry in the browser's history, without reloading
            window.history.pushState(nextState, nextTitle, nextURL);
        },
        GetTabByURL(){
            var url = window.location.href;
            url_split = url.split("/");
            var _tab = url_split[url_split.length - 1];
            switch (_tab){
                case "dashboard":
                    return 0;
                case "times":
                    return 1;
                case "trails":
                    return 2;
            }
            app.SilentUrlSwitch('dashboard')
            return 2;
        },
        GetPlayerOutputLog(player){
            $.get("/get-output-log/" + player.id, function(data){
                app.cached_output_log = data;
            });
        },
        GetTimeDiff(time, trail, index){
            try{
                if (index == 0)
                    return 0;
                return (time - trail.leaderboard[0].time).toFixed(2);
            } catch(Exception){}
        },
        GetTrails(){
            $.get("/get-trails", function(data){
                app.trails = data["trails"];
                app.trails.sort(function(a, b){return a.world_name-b.world_name});
            })
        },
        UpdateTimeIgnore(time){
            $.get("/ignore-time/" + time.time_id + "/" + time.ignore, function(text){
                // if failed, revert to previous time ignore.
                if (text != "success")
                    time.ignore = app.InverseStringBool(time.ignore);
            });
        },
        colorFromNameUsingHash(name) {
            // Simple hash function to generate a color based on the input name
            const hashCode = name.split('').reduce((acc, char) => {
                return char.charCodeAt(0) + acc;
            }, 0);
            
            // Convert the hash code to a hexadecimal color value
            const colorValue = `#${(hashCode & 0x00ffffff).toString(16)}`;
            
            return colorValue;
        },
        InverseStringBool(value){
            if (value == "True")
                return "False";
            else
                return "True";
        },
        GetWorlds(){
            $.get("/get-worlds", function(data){
                app.worlds = data["worlds"];
            })
        },
        CheckStatus(){
            $.get("/permission", function(data){
                if (data != app.validated && data == "AUTHORISED"){
                    app.tab = 0;
                }
                app.validated = data;
            })
        },
        toHours(value){
            const sec = parseInt(value, 10); // convert value to number if it's string
            let hours   = Math.floor(sec / 3600); // get hours
            let minutes = Math.floor((sec - (hours * 3600)) / 60); // get minutes
            let seconds = sec - (hours * 3600) - (minutes * 60); //  get seconds
            // add 0 if value < 10; Example: 2 => 02
            if (hours   < 10) {hours   = "0"+hours;}
            if (minutes < 10) {minutes = "0"+minutes;}
            if (seconds < 10) {seconds = "0"+seconds;}
            return hours+' hrs, '+minutes+' mins, '+seconds+' secs'; // Return is HH : MM : SS
        },
        timeFromTimestamp(stamp){
            return (Date.now()/1000) - stamp
        },
        SearchMatch(search, item){
            if (search == null || search == "")
                return true;
            if (item.name.toLowerCase().includes(search.toLowerCase()))
                return true;
        },
        Spectate(id){
            $.get("/api/spectate", data={
                "target_id": id
            });
        },
        timestamp_to_date(unix_timestamp){
            var s = new Date(unix_timestamp * 1000).toLocaleDateString("en-UK")
            return s;
        },
        timestamp_to_date_time(unix_timestamp){
            var date = new Date(unix_timestamp * 1000);
            var options = { year: 'numeric', month: 'numeric', day: 'numeric', hour: 'numeric', minute: 'numeric', second: 'numeric' };
            var s = date.toLocaleDateString("en-UK", options);
            return s;
        },
        secs_to_str(secs){
            secs = parseFloat(secs);
            d_mins = Math.floor(secs / 60);
            d_secs = Math.floor(secs % 60)
            fraction = secs * 1000;
            fraction = Math.round(fraction % 1000);
            if (fraction > 999){
              d_secs += 1;
              fraction = 0;
            }
            d_mins = d_mins.toString();
            d_secs = d_secs.toString();
            fraction = fraction.toString();
            if (d_mins.length == 1)
                d_mins = "0" + d_mins.toString()
            if (d_secs.length == 1)
                d_secs = "0" + d_secs
            while (fraction.length < 3)
                fraction = "0" + fraction
            return d_mins + ":" + d_secs + "." + fraction
        },
        redirect(url){
            window.location.href = url;
        },
        toggle_ignore_time(time_id, val){
            $.get("/toggle-ignore-time/" + time_id, data={"val": val});
        },
        getLeaderboard(){
            app.times = [];
            $.get("/get-all-times", data={"lim": 100}, function(data){
                app.times = data["times"];
            })
        },
        getColourOfBikeSelect(ourBike, setBike){
            if (ourBike == setBike){
                return "green"
            }
            return "black"
        },
        getSteamId(){
            $.get("/me", function(data){
                if (data.connections.code != 0){
                    data.connections.forEach(function(val){
                        if (val["type"] == "steam"){
                            app.self = val["id"];
                        }
                    })
                }
            })
        },
        addCommaToNum(num){
            if (num != null){
                return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
            }
            else{
                return ""
            }
        },
        updatePlayers() {
            $.getJSON("/get", function(data){
                
                function compare_lname( a, b )
                {
                    if (a.name != null && b.name != null){
                        if ( a.name.toLowerCase() < b.name.toLowerCase()){
                            return -1;
                        }
                        if ( a.name.toLowerCase() > b.name.toLowerCase()){
                            return 1;
                        }
                    }
                    return 0;
                }
                //data["players"].sort(compare_lname);
                app.players = data["players"];
                if (app.players[0] == null || app.players[0].id != "ALL"){
                    app.players.unshift(
                        {
                            "name":"All Players",
                            "id" : "ALL",
                            "command" : "",
                            "address": ["0.0.0.0", 6969],
                            "steam_avatar_src" : "https://dinahjean.files.wordpress.com/2020/04/all.jpg",
                            "time_on_world": 0,
                            "total_time": 0,
                            "time_loaded": startTime/1000,
                            "world_name": "this dashboard",
                            "reputation":  0,
                            "version": "0.0.0",
                            "bike_type": ""
                        }
                    )
                }
            })
        }
    }
});

let startTime = new Date().getTime();

//app.setSelf('UNKNOWN');
app.updatePlayers();
app.CheckStatus();
//setInterval(updatePlayers, 1000);
//setInterval(app.CheckStatus, 1000);
app.getSteamId();
//setInterval(app.getSteamId, 500);
//app.GetConcurrency();
app.GetTrails();
//app.GetWorlds();
app.getLeaderboard();
app.tab = app.GetTabByURL();

app.items = [
    {
        "id": "1445",
        "name": "Air Horn"
    },
    {
        "id": "1146",
        "name": "Arboreal Horn"
    },
    {
        "id": "1172",
        "name": "Arboreal Racing Plate"
    },
    {
        "id": "1237",
        "name": "CashCow Bell"
    },
    {
        "id": "1255",
        "name": "Christmas Bells"
    },
    {
        "id": "1232",
        "name": "Christmas Lights"
    },
    {
        "id": "1447",
        "name": "Clown Horn Doofy"
    },
    {
        "id": "1446",
        "name": "Clown Horn Doopy"
    },
    {
        "id": "1134",
        "name": "Descenders Bell"
    },
    {
        "id": "1144",
        "name": "Enemy Horn"
    },
    {
        "id": "1173",
        "name": "Enemy Racing Plate"
    },
    {
        "id": "1393",
        "name": "FortyTwo Amber Racing Plate"
    },
    {
        "id": "1394",
        "name": "FortyTwo Apple Racing Plate"
    },
    {
        "id": "1395",
        "name": "FortyTwo Aqua Racing Plate"
    },
    {
        "id": "1396",
        "name": "FortyTwo Cobalt Racing Plate"
    },
    {
        "id": "1397",
        "name": "FortyTwo Frost Racing Plate"
    },
    {
        "id": "1398",
        "name": "FortyTwo Neon Racing Plate"
    },
    {
        "id": "1399",
        "name": "FortyTwo Royal Racing Plate"
    },
    {
        "id": "1392",
        "name": "FortyTwo Vermillion Racing Plate"
    },
    {
        "id": "1133",
        "name": "Gold Horn"
    },
    {
        "id": "1145",
        "name": "Kinetic Horn"
    },
    {
        "id": "1174",
        "name": "Kinetic Racing Plate"
    },
    {
        "id": "999",
        "name": "None"
    },
    {
        "id": "1171",
        "name": "Pinwheel"
    },
    {
        "id": "1228",
        "name": "Pizza"
    },
    {
        "id": "1184",
        "name": "Police Beacon"
    },
    {
        "id": "1256",
        "name": "Pro Send It Camera"
    },
    {
        "id": "1647",
        "name": "Rubber Cat"
    },
    {
        "id": "1649",
        "name": "Rubber Cow"
    },
    {
        "id": "1187",
        "name": "Rubber Doggy"
    },
    {
        "id": "188",
        "name": "Rubber Ducky"
    },
    {
        "id": "1650",
        "name": "Rubber Frog"
    },
    {
        "id": "1651",
        "name": "Rubber Piggy"
    },
    {
        "id": "1648",
        "name": "Rubber Sheep"
    },
    {
        "id": "1189",
        "name": "Sunflower Basket"
    },
    {
        "id": "1227",
        "name": "Teddy"
    },
    {
        "id": "1258",
        "name": "Vuvuzela"
    },
    {
        "id": "1238",
        "name": "Whoopee Cushion"
    },
    {
        "id": "1280",
        "name": "Betsy"
    },
    {
        "id": "1283",
        "name": "Cedar"
    },
    {
        "id": "1279",
        "name": "Henk"
    },
    {
        "id": "1282",
        "name": "Kentony"
    },
    {
        "id": "1281",
        "name": "Neil"
    },
    {
        "id": "1161",
        "name": "Antlers"
    },
    {
        "id": "1224",
        "name": "Bat Wings"
    },
    {
        "id": "1627",
        "name": "Bear"
    },
    {
        "id": "1158",
        "name": "Bear Ears"
    },
    {
        "id": "1244",
        "name": "Blindfold"
    },
    {
        "id": "1151",
        "name": "Blue Mohawk"
    },
    {
        "id": "1387",
        "name": "Buckethead"
    },
    {
        "id": "1225",
        "name": "Butcher Knife"
    },
    {
        "id": "1239",
        "name": "CashCow Mask"
    },
    {
        "id": "1115",
        "name": "Chicken Mask"
    },
    {
        "id": "1136",
        "name": "Descenders Crown"
    },
    {
        "id": "1624",
        "name": "Dino"
    },
    {
        "id": "1620",
        "name": "Drink Holder"
    },
    {
        "id": "1386",
        "name": "Dunce Hat"
    },
    {
        "id": "1643",
        "name": "Flamingo"
    },
    {
        "id": "1159",
        "name": "Fox Ears"
    },
    {
        "id": "1631",
        "name": "Giraffe"
    },
    {
        "id": "1152",
        "name": "Green Mohawk"
    },
    {
        "id": "1157",
        "name": "Halo"
    },
    {
        "id": "1186",
        "name": "Heli Cap"
    },
    {
        "id": "181",
        "name": "Horse Mask"
    },
    {
        "id": "998",
        "name": "None"
    },
    {
        "id": "1637",
        "name": "Penguin"
    },
    {
        "id": "1153",
        "name": "Pink Mohawk"
    },
    {
        "id": "1257",
        "name": "Pro Send It Camera"
    },
    {
        "id": "1226",
        "name": "Pumpkin Head"
    },
    {
        "id": "1160",
        "name": "Rabbit Ears"
    },
    {
        "id": "1150",
        "name": "Red Mohawk"
    },
    {
        "id": "1634",
        "name": "Rhino"
    },
    {
        "id": "1251",
        "name": "Santa Hat"
    },
    {
        "id": "1640",
        "name": "Shark"
    },
    {
        "id": "1619",
        "name": "Sombrero"
    },
    {
        "id": "1385",
        "name": "Space Helmet"
    },
    {
        "id": "1246",
        "name": "Squid Mask"
    },
    {
        "id": "1621",
        "name": "Taco"
    },
    {
        "id": "1156",
        "name": "Top Hat"
    },
    {
        "id": "1672",
        "name": "Toxic Mask"
    },
    {
        "id": "1646",
        "name": "Unicorn"
    },
    {
        "id": "1162",
        "name": "Viking Helmet"
    },
    {
        "id": "1154",
        "name": "Yellow Mohawk"
    },
    {
        "id": "1143",
        "name": "Arboreal Flag"
    },
    {
        "id": "1166",
        "name": "Arboreal Token"
    },
    {
        "id": "1351",
        "name": "Bananas"
    },
    {
        "id": "1168",
        "name": "Beach Ball"
    },
    {
        "id": "1253",
        "name": "Christmas Gift"
    },
    {
        "id": "1222",
        "name": "Coffin"
    },
    {
        "id": "1169",
        "name": "Corn"
    },
    {
        "id": "1064",
        "name": "Afghan Flag"
    },
    {
        "id": "1065",
        "name": "Algerian Flag"
    },
    {
        "id": "1063",
        "name": "Canadian Flag"
    },
    {
        "id": "1400",
        "name": "Cycling Bags"
    },
    {
        "id": "1135",
        "name": "Descenders Flag"
    },
    {
        "id": "1141",
        "name": "Enemy Flag"
    },
    {
        "id": "1164",
        "name": "Enemy Token"
    },
    {
        "id": "1170",
        "name": "Eye Ball"
    },
    {
        "id": "1247",
        "name": "Gingerbread Man"
    },
    {
        "id": "1080",
        "name": "Heart"
    },
    {
        "id": "1142",
        "name": "Kinetic Flag"
    },
    {
        "id": "1165",
        "name": "Kinetic Token"
    },
    {
        "id": "1218",
        "name": "Lux Arika Reflector"
    },
    {
        "id": "1219",
        "name": "Lux Cya Reflector"
    },
    {
        "id": "1221",
        "name": "Lux Genta Reflector"
    },
    {
        "id": "1217",
        "name": "Lux Musa Reflector"
    },
    {
        "id": "1220",
        "name": "Lux Verda Reflector"
    },
    {
        "id": "997",
        "name": "None"
    },
    {
        "id": "1167",
        "name": "OK Hand"
    },
    {
        "id": "1628",
        "name": "Pinata"
    },
    {
        "id": "1653",
        "name": "Aromantic Flag"
    },
    {
        "id": "1654",
        "name": "Asexual Flag"
    },
    {
        "id": "1655",
        "name": "Bisexual Flag"
    },
    {
        "id": "1656",
        "name": "Demisexual Flag"
    },
    {
        "id": "1657",
        "name": "Gay Flag"
    },
    {
        "id": "1658",
        "name": "Genderqueer Flag"
    },
    {
        "id": "1659",
        "name": "Intersex Flag"
    },
    {
        "id": "1660",
        "name": "Lesbian Flag"
    },
    {
        "id": "1661",
        "name": "Nonbinary Flag"
    },
    {
        "id": "1662",
        "name": "Pansexual Flag"
    },
    {
        "id": "1663",
        "name": "Polyamory Flag"
    },
    {
        "id": "1185",
        "name": "Rainbow Flag"
    },
    {
        "id": "1664",
        "name": "Transgender Flag"
    },
    {
        "id": "1388",
        "name": "Train"
    },
    {
        "id": "1188",
        "name": "Training Set"
    },
    {
        "id": "1513",
        "name": "Yarr Flag"
    },
    {
        "id": "1149",
        "name": "Arboreal Bike"
    },
    {
        "id": "179",
        "name": "BS Inspired Bike"
    },
    {
        "id": "1235",
        "name": "Cashcow Bike"
    },
    {
        "id": "1346",
        "name": "Ameleo Bike"
    },
    {
        "id": "1331",
        "name": "APEX Bike"
    },
    {
        "id": "1309",
        "name": "Argardnaut Meltdown Bike"
    },
    {
        "id": "1320",
        "name": "Arrow Bike"
    },
    {
        "id": "1673",
        "name": "Aura Bike"
    },
    {
        "id": "1601",
        "name": "Bronze Bike"
    },
    {
        "id": "1686",
        "name": "Bullion Bike"
    },
    {
        "id": "1314",
        "name": "Contrast Bike"
    },
    {
        "id": "1681",
        "name": "Cycledelic Bike"
    },
    {
        "id": "1361",
        "name": "Evil One Bike"
    },
    {
        "id": "1376",
        "name": "Factory Blue Bike"
    },
    {
        "id": "1378",
        "name": "Factory Green Bike"
    },
    {
        "id": "1377",
        "name": "Factory Red Bike"
    },
    {
        "id": "1391",
        "name": "Fire and Ice Bike"
    },
    {
        "id": "1313",
        "name": "Flamingo Bike"
    },
    {
        "id": "1599",
        "name": "Gold Bike"
    },
    {
        "id": "1363",
        "name": "Headshot Bike"
    },
    {
        "id": "1362",
        "name": "HellTrek Bike"
    },
    {
        "id": "1326",
        "name": "Hellwave Bike"
    },
    {
        "id": "1369",
        "name": "High Voltage Bike"
    },
    {
        "id": "1343",
        "name": "Hipwood Iron Bike"
    },
    {
        "id": "1344",
        "name": "Hipwood Vintage Bike"
    },
    {
        "id": "1665",
        "name": "Igloo Bike"
    },
    {
        "id": "1345",
        "name": "PIPex Butterfly Bike"
    },
    {
        "id": "1390",
        "name": "PIPex Ladybug Bike"
    },
    {
        "id": "1364",
        "name": "RIDE Bike"
    },
    {
        "id": "1600",
        "name": "Silver Bike"
    },
    {
        "id": "1389",
        "name": "Untextured Bike"
    },
    {
        "id": "1325",
        "name": "WolfPak Aka Bike"
    },
    {
        "id": "1336",
        "name": "Zeron Bike"
    },
    {
        "id": "1514",
        "name": "Custom 01 Bike"
    },
    {
        "id": "1515",
        "name": "Custom 02 Bike"
    },
    {
        "id": "1516",
        "name": "Custom 03 Bike"
    },
    {
        "id": "1517",
        "name": "Custom 04 Bike"
    },
    {
        "id": "1518",
        "name": "Custom 05 Bike"
    },
    {
        "id": "1519",
        "name": "Custom 06 Bike"
    },
    {
        "id": "1520",
        "name": "Custom 07 Bike"
    },
    {
        "id": "1521",
        "name": "Custom 08 Bike"
    },
    {
        "id": "1522",
        "name": "Custom 09 Bike"
    },
    {
        "id": "1523",
        "name": "Custom 10 Bike"
    },
    {
        "id": "1584",
        "name": "Custom 11 Bike"
    },
    {
        "id": "1585",
        "name": "Custom 12 Bike"
    },
    {
        "id": "1586",
        "name": "Custom 13 Bike"
    },
    {
        "id": "1587",
        "name": "Custom 14 Bike"
    },
    {
        "id": "1588",
        "name": "Custom 15 Bike"
    },
    {
        "id": "1589",
        "name": "Custom 16 Bike"
    },
    {
        "id": "1590",
        "name": "Custom 17 Bike"
    },
    {
        "id": "1591",
        "name": "Custom 18 Bike"
    },
    {
        "id": "1592",
        "name": "Custom 19 Bike"
    },
    {
        "id": "1593",
        "name": "Custom 20 Bike"
    },
    {
        "id": "1594",
        "name": "Custom 21 Bike"
    },
    {
        "id": "1595",
        "name": "Custom 22 Bike"
    },
    {
        "id": "1596",
        "name": "Custom 23 Bike"
    },
    {
        "id": "1597",
        "name": "Custom 24 Bike"
    },
    {
        "id": "1598",
        "name": "Custom 25 Bike"
    },
    {
        "id": "1066",
        "name": "Discord Bike"
    },
    {
        "id": "1147",
        "name": "Enemy Bike"
    },
    {
        "id": "22",
        "name": "Flat Basic Bike"
    },
    {
        "id": "72",
        "name": "Flat Caution Orange Bike"
    },
    {
        "id": "69",
        "name": "Flat Flash Yellow Bike"
    },
    {
        "id": "70",
        "name": "Flat Fury Red Bike"
    },
    {
        "id": "73",
        "name": "Flat Lime Green Bike"
    },
    {
        "id": "74",
        "name": "Flat Marvelous Magenta Bike"
    },
    {
        "id": "68",
        "name": "Flat Onyx Black Bike"
    },
    {
        "id": "71",
        "name": "Flat Triumph Blue Bike"
    },
    {
        "id": "1155",
        "name": "InvisiBike"
    },
    {
        "id": "1148",
        "name": "Kinetic Bike"
    },
    {
        "id": "1132",
        "name": "Lux Banana Bike"
    },
    {
        "id": "1214",
        "name": "Lux Descent Bike"
    },
    {
        "id": "1130",
        "name": "Lux District Bike"
    },
    {
        "id": "1129",
        "name": "Lux Lantern Bike"
    },
    {
        "id": "1131",
        "name": "Lux Legacy Bike"
    },
    {
        "id": "1688",
        "name": "Lux Rainbow Bike"
    },
    {
        "id": "1310",
        "name": "Lynx D1 Bike"
    },
    {
        "id": "1312",
        "name": "Lynx G7 Bike"
    },
    {
        "id": "1311",
        "name": "Lynx X5 Bike"
    },
    {
        "id": "1308",
        "name": "Lynx Z2 Bike"
    },
    {
        "id": "1687",
        "name": "MISSING MATERIAL"
    },
    {
        "id": "1491",
        "name": "Dogtorque 2019 Bike"
    },
    {
        "id": "1489",
        "name": "KingKrautz 2019 Bike"
    },
    {
        "id": "1490",
        "name": "Spe 2019 Bike"
    },
    {
        "id": "1193",
        "name": "Newline Admiral Bike"
    },
    {
        "id": "1198",
        "name": "Newline Ashen Bike"
    },
    {
        "id": "1197",
        "name": "Newline Imperial Bike"
    },
    {
        "id": "1190",
        "name": "Newline Jade Bike"
    },
    {
        "id": "1196",
        "name": "Newline Ochre Bike"
    },
    {
        "id": "1192",
        "name": "Newline Orchid Bike"
    },
    {
        "id": "1194",
        "name": "Newline Sapphire Bike"
    },
    {
        "id": "1191",
        "name": "Newline Scarlet Bike"
    },
    {
        "id": "1195",
        "name": "Newline Xanthous Bike"
    },
    {
        "id": "142",
        "name": "Rusty Bike"
    },
    {
        "id": "1098",
        "name": "Stealth Bike"
    },
    {
        "id": "1022",
        "name": "Stickerbomb Bike"
    },
    {
        "id": "1306",
        "name": "Sukurabu B Bike"
    },
    {
        "id": "1304",
        "name": "Sukurabu D Bike"
    },
    {
        "id": "1307",
        "name": "Sukurabu F Bike"
    },
    {
        "id": "1303",
        "name": "Sukurabu G Bike"
    },
    {
        "id": "1305",
        "name": "Sukurabu W Bike"
    },
    {
        "id": "1302",
        "name": "Sukurabu X Bike"
    },
    {
        "id": "1025",
        "name": "T2 Amethyst Bike"
    },
    {
        "id": "1020",
        "name": "T2 Electric Bike"
    },
    {
        "id": "1021",
        "name": "T2 Formula Bike"
    },
    {
        "id": "92",
        "name": "T2 Goblin Bike"
    },
    {
        "id": "93",
        "name": "T2 Life Bike"
    },
    {
        "id": "1024",
        "name": "T2 Sakura Bike"
    },
    {
        "id": "1023",
        "name": "T2 Skyline Bike"
    },
    {
        "id": "62",
        "name": "T2 Trooper Bike"
    },
    {
        "id": "63",
        "name": "T3 Harmony Bike"
    },
    {
        "id": "66",
        "name": "T3 Nebula Bike"
    },
    {
        "id": "65",
        "name": "T3 Rasta Bike"
    },
    {
        "id": "1088",
        "name": "T3 Shot Bike"
    },
    {
        "id": "64",
        "name": "T3 Sunrise Bike"
    },
    {
        "id": "141",
        "name": "Twitch Bike"
    },
    {
        "id": "80",
        "name": "Arboreal Master Goggles"
    },
    {
        "id": "81",
        "name": "Arboreal Novice Goggles"
    },
    {
        "id": "82",
        "name": "Arboreal Pro Goggles"
    },
    {
        "id": "1321",
        "name": "Arrow Goggles"
    },
    {
        "id": "1674",
        "name": "Aura Goggles"
    },
    {
        "id": "1604",
        "name": "Bronze Goggles"
    },
    {
        "id": "1682",
        "name": "Bullion Goggles"
    },
    {
        "id": "1315",
        "name": "Contrast Goggles"
    },
    {
        "id": "1441",
        "name": "Glocksee Goggles"
    },
    {
        "id": "1602",
        "name": "Gold Goggles"
    },
    {
        "id": "1332",
        "name": "Headshot Goggles"
    },
    {
        "id": "1327",
        "name": "Hellwave Goggles"
    },
    {
        "id": "1379",
        "name": "High Voltage Goggles"
    },
    {
        "id": "1340",
        "name": "Hipwood Leaf Goggles"
    },
    {
        "id": "1341",
        "name": "Hipwood Magma Goggles"
    },
    {
        "id": "1342",
        "name": "Hipwood Snow Goggles"
    },
    {
        "id": "1666",
        "name": "Igloo Goggles"
    },
    {
        "id": "1347",
        "name": "Poison Dart Goggles"
    },
    {
        "id": "1357",
        "name": "Rascal Goggles"
    },
    {
        "id": "1365",
        "name": "RIDE Goggles"
    },
    {
        "id": "1603",
        "name": "Silver Goggles"
    },
    {
        "id": "1370",
        "name": "Zeron Goggles"
    },
    {
        "id": "1524",
        "name": "Custom 01 Goggles"
    },
    {
        "id": "1525",
        "name": "Custom 02 Goggles"
    },
    {
        "id": "1526",
        "name": "Custom 03 Goggles"
    },
    {
        "id": "1527",
        "name": "Custom 04 Goggles"
    },
    {
        "id": "1528",
        "name": "Custom 05 Goggles"
    },
    {
        "id": "1529",
        "name": "Custom 06 Goggles"
    },
    {
        "id": "1530",
        "name": "Custom 07 Goggles"
    },
    {
        "id": "1531",
        "name": "Custom 08 Goggles"
    },
    {
        "id": "1532",
        "name": "Custom 09 Goggles"
    },
    {
        "id": "1533",
        "name": "Custom 10 Goggles"
    },
    {
        "id": "1070",
        "name": "Descenders Goggles"
    },
    {
        "id": "1076",
        "name": "Descenders Legendary Goggles"
    },
    {
        "id": "1353",
        "name": "Descenders Master Goggles"
    },
    {
        "id": "1003",
        "name": "Liquicity Goggles"
    },
    {
        "id": "1008",
        "name": "No More Robots Goggles"
    },
    {
        "id": "1016",
        "name": "Power Up Audio Goggles"
    },
    {
        "id": "1012",
        "name": "RageSquid Goggles"
    },
    {
        "id": "43",
        "name": "Enemy Master Goggles"
    },
    {
        "id": "95",
        "name": "Enemy Novice Goggles"
    },
    {
        "id": "96",
        "name": "Enemy Pro Goggles"
    },
    {
        "id": "13",
        "name": "Ether Basic Goggles"
    },
    {
        "id": "48",
        "name": "Ether Caution Orange Goggles"
    },
    {
        "id": "45",
        "name": "Ether Flash Yellow Goggles"
    },
    {
        "id": "46",
        "name": "Ether Fury Red Goggles"
    },
    {
        "id": "49",
        "name": "Ether Lime Green Goggles"
    },
    {
        "id": "50",
        "name": "Ether Marvelous Magenta Goggles"
    },
    {
        "id": "44",
        "name": "Ether Onyx Black Goggles"
    },
    {
        "id": "47",
        "name": "Ether Triumph Blue Goggles"
    },
    {
        "id": "1029",
        "name": "FortyTwo Amber Goggles"
    },
    {
        "id": "1111",
        "name": "FortyTwo Apex Goggles"
    },
    {
        "id": "1030",
        "name": "FortyTwo Apple Goggles"
    },
    {
        "id": "1031",
        "name": "FortyTwo Aqua Goggles"
    },
    {
        "id": "1028",
        "name": "FortyTwo Cobalt Goggles"
    },
    {
        "id": "1114",
        "name": "FortyTwo Crank Goggles"
    },
    {
        "id": "1026",
        "name": "FortyTwo Frost Goggles"
    },
    {
        "id": "1032",
        "name": "FortyTwo Neon Goggles"
    },
    {
        "id": "1059",
        "name": "FortyTwo Rasta Goggles"
    },
    {
        "id": "1112",
        "name": "FortyTwo Rival Goggles"
    },
    {
        "id": "1033",
        "name": "FortyTwo Royal Goggles"
    },
    {
        "id": "1113",
        "name": "FortyTwo Ultra Goggles"
    },
    {
        "id": "1027",
        "name": "FortyTwo Vermillion Goggles"
    },
    {
        "id": "169",
        "name": "High Rollers Goggles"
    },
    {
        "id": "25",
        "name": "Kinetic Master Goggles"
    },
    {
        "id": "23",
        "name": "Kinetic Novice Goggles"
    },
    {
        "id": "24",
        "name": "Kinetic Pro Goggles"
    },
    {
        "id": "1259",
        "name": "Lux Banana Goggles"
    },
    {
        "id": "1263",
        "name": "Lux Descent Goggles"
    },
    {
        "id": "1262",
        "name": "Lux District Goggles"
    },
    {
        "id": "1261",
        "name": "Lux Lantern Goggles"
    },
    {
        "id": "1260",
        "name": "Lux Legacy Goggles"
    },
    {
        "id": "1123",
        "name": "Mixer Goggles"
    },
    {
        "id": "1494",
        "name": "Dogtorque 2019 Goggles"
    },
    {
        "id": "1492",
        "name": "KingKrautz 2019 Goggles"
    },
    {
        "id": "1493",
        "name": "Spe 2019 Goggles"
    },
    {
        "id": "1452",
        "name": "Australia Goggles"
    },
    {
        "id": "1480",
        "name": "Austria Goggles"
    },
    {
        "id": "1401",
        "name": "Canada Goggles"
    },
    {
        "id": "1402",
        "name": "China Goggles"
    },
    {
        "id": "1479",
        "name": "Estonia Goggles"
    },
    {
        "id": "1456",
        "name": "Finland Goggles"
    },
    {
        "id": "1403",
        "name": "France Goggles"
    },
    {
        "id": "1404",
        "name": "Germany Goggles"
    },
    {
        "id": "1405",
        "name": "Japan Goggles"
    },
    {
        "id": "1406",
        "name": "Netherlands Goggles"
    },
    {
        "id": "1463",
        "name": "New Zealand Goggles"
    },
    {
        "id": "1464",
        "name": "Portugal Goggles"
    },
    {
        "id": "1471",
        "name": "Russia Goggles"
    },
    {
        "id": "1407",
        "name": "South Africa Goggles"
    },
    {
        "id": "1481",
        "name": "Spain Goggles"
    },
    {
        "id": "1472",
        "name": "Sweden Goggles"
    },
    {
        "id": "1408",
        "name": "United Kingdom Goggles"
    },
    {
        "id": "1410",
        "name": "USA Goggles"
    },
    {
        "id": "1409",
        "name": "Wales Goggles"
    },
    {
        "id": "1504",
        "name": "Nintendo Switch Goggles"
    },
    {
        "id": "1505",
        "name": "PS4 Goggles"
    },
    {
        "id": "1118",
        "name": "Scrape Camo Goggles"
    },
    {
        "id": "1117",
        "name": "Scrape Skull Goggles"
    },
    {
        "id": "1116",
        "name": "Scrape Thunder Goggles"
    },
    {
        "id": "1128",
        "name": "Xbox Goggles"
    },
    {
        "id": "83",
        "name": "Arboreal Master Helmet"
    },
    {
        "id": "84",
        "name": "Arboreal Novice Helmet"
    },
    {
        "id": "85",
        "name": "Arboreal Pro Helmet"
    },
    {
        "id": "1293",
        "name": "Camo+ DH Helmet"
    },
    {
        "id": "1290",
        "name": "Camo+ GT Helmet"
    },
    {
        "id": "1292",
        "name": "Camo+ RI Helmet"
    },
    {
        "id": "1291",
        "name": "Camo+ XS Helmet"
    },
    {
        "id": "1000",
        "name": "Camo Blue Helmet"
    },
    {
        "id": "159",
        "name": "Camo Grey Helmet"
    },
    {
        "id": "161",
        "name": "Camo Lime Helmet"
    },
    {
        "id": "162",
        "name": "Camo Red Helmet"
    },
    {
        "id": "1298",
        "name": "Carbon Axe Helmet"
    },
    {
        "id": "1301",
        "name": "Carbon Crave Helmet"
    },
    {
        "id": "1299",
        "name": "Carbon Lazer Helmet"
    },
    {
        "id": "1300",
        "name": "Carbon Slice Helmet"
    },
    {
        "id": "1322",
        "name": "Arrow Helmet"
    },
    {
        "id": "1675",
        "name": "Aura Helmet"
    },
    {
        "id": "1607",
        "name": "Bronze Helmet"
    },
    {
        "id": "1683",
        "name": "Bullion Helmet"
    },
    {
        "id": "1316",
        "name": "Contrast Helmet"
    },
    {
        "id": "1678",
        "name": "Cycledelic Helmet"
    },
    {
        "id": "1442",
        "name": "Glocksee Helmet"
    },
    {
        "id": "1605",
        "name": "Gold Helmet"
    },
    {
        "id": "1333",
        "name": "Headshot Helmet"
    },
    {
        "id": "1328",
        "name": "Hellwave Helmet"
    },
    {
        "id": "1380",
        "name": "High Voltage Helmet"
    },
    {
        "id": "1337",
        "name": "Hipwood Leaf Helmet"
    },
    {
        "id": "1338",
        "name": "Hipwood Magma Helmet"
    },
    {
        "id": "1339",
        "name": "Hipwood Snow Helmet"
    },
    {
        "id": "1667",
        "name": "Igloo Helmet"
    },
    {
        "id": "1348",
        "name": "Poison Dart Helmet"
    },
    {
        "id": "1358",
        "name": "Rascal Helmet"
    },
    {
        "id": "1366",
        "name": "RIDE Helmet"
    },
    {
        "id": "1606",
        "name": "Silver Helmet"
    },
    {
        "id": "1671",
        "name": "Toxic Helmet"
    },
    {
        "id": "1371",
        "name": "Zeron Helmet"
    },
    {
        "id": "1534",
        "name": "Custom 01 Helmet"
    },
    {
        "id": "1535",
        "name": "Custom 02 Helmet"
    },
    {
        "id": "1536",
        "name": "Custom 03 Helmet"
    },
    {
        "id": "1537",
        "name": "Custom 04 Helmet"
    },
    {
        "id": "1538",
        "name": "Custom 05 Helmet"
    },
    {
        "id": "1539",
        "name": "Custom 06 Helmet"
    },
    {
        "id": "1540",
        "name": "Custom 07 Helmet"
    },
    {
        "id": "1541",
        "name": "Custom 08 Helmet"
    },
    {
        "id": "1542",
        "name": "Custom 09 Helmet"
    },
    {
        "id": "1543",
        "name": "Custom 10 Helmet"
    },
    {
        "id": "1075",
        "name": "Descenders Helmet"
    },
    {
        "id": "1077",
        "name": "Descenders Legendary Helmet"
    },
    {
        "id": "1354",
        "name": "Descenders Master Helmet"
    },
    {
        "id": "1001",
        "name": "Liquicity Helmet"
    },
    {
        "id": "1009",
        "name": "No More Robots Helmet"
    },
    {
        "id": "1017",
        "name": "Power Up Audio Helmet"
    },
    {
        "id": "1014",
        "name": "RageSquid Helmet"
    },
    {
        "id": "97",
        "name": "Enemy Master Helmet"
    },
    {
        "id": "98",
        "name": "Enemy Novice Helmet"
    },
    {
        "id": "99",
        "name": "Enemy Pro Helmet"
    },
    {
        "id": "14",
        "name": "Ether Basic Helmet"
    },
    {
        "id": "56",
        "name": "Ether Caution Orange Helmet"
    },
    {
        "id": "53",
        "name": "Ether Flash Yellow Helmet"
    },
    {
        "id": "54",
        "name": "Ether Fury Red Helmet"
    },
    {
        "id": "57",
        "name": "Ether Lime Green Helmet"
    },
    {
        "id": "58",
        "name": "Ether Marvelous Magenta Helmet"
    },
    {
        "id": "52",
        "name": "Ether Onyx Black Helmet"
    },
    {
        "id": "55",
        "name": "Ether Triumph Blue Helmet"
    },
    {
        "id": "1294",
        "name": "Fade DWN Helmet"
    },
    {
        "id": "1296",
        "name": "Fade EVG Helmet"
    },
    {
        "id": "1297",
        "name": "Fade OHO Helmet"
    },
    {
        "id": "1295",
        "name": "Fade SRS Helmet"
    },
    {
        "id": "1037",
        "name": "FortyTwo Amber Helmet"
    },
    {
        "id": "1107",
        "name": "FortyTwo Apex Helmet"
    },
    {
        "id": "1038",
        "name": "FortyTwo Apple Helmet"
    },
    {
        "id": "1039",
        "name": "FortyTwo Aqua Helmet"
    },
    {
        "id": "1036",
        "name": "FortyTwo Cobalt Helmet"
    },
    {
        "id": "1110",
        "name": "FortyTwo Crank Helmet"
    },
    {
        "id": "1034",
        "name": "FortyTwo Frost Helmet"
    },
    {
        "id": "1040",
        "name": "FortyTwo Neon Helmet"
    },
    {
        "id": "1060",
        "name": "FortyTwo Rasta Helmet"
    },
    {
        "id": "1108",
        "name": "FortyTwo Rival Helmet"
    },
    {
        "id": "1041",
        "name": "FortyTwo Royal Helmet"
    },
    {
        "id": "1109",
        "name": "FortyTwo Ultra Helmet"
    },
    {
        "id": "1035",
        "name": "FortyTwo Vermillion Helmet"
    },
    {
        "id": "170",
        "name": "High Rollers Helmet"
    },
    {
        "id": "29",
        "name": "Kinetic Master Helmet"
    },
    {
        "id": "27",
        "name": "Kinetic Novice Helmet"
    },
    {
        "id": "28",
        "name": "Kinetic Pro Helmet"
    },
    {
        "id": "1264",
        "name": "Lux Banana Helmet"
    },
    {
        "id": "1268",
        "name": "Lux Descent Helmet"
    },
    {
        "id": "1265",
        "name": "Lux District Helmet"
    },
    {
        "id": "1267",
        "name": "Lux Lantern Helmet"
    },
    {
        "id": "1266",
        "name": "Lux Legacy Helmet"
    },
    {
        "id": "1124",
        "name": "Mixer Helmet"
    },
    {
        "id": "1497",
        "name": "Dogtorque 2019 Helmet"
    },
    {
        "id": "1495",
        "name": "KingKrautz 2019 Helmet"
    },
    {
        "id": "1496",
        "name": "Spe 2019 Helmet"
    },
    {
        "id": "1453",
        "name": "Australia Helmet"
    },
    {
        "id": "1482",
        "name": "Austria Helmet"
    },
    {
        "id": "1411",
        "name": "Canada Helmet"
    },
    {
        "id": "1412",
        "name": "China Helmet"
    },
    {
        "id": "1478",
        "name": "Estonia Helmet"
    },
    {
        "id": "1457",
        "name": "Finland Helmet"
    },
    {
        "id": "1413",
        "name": "France Helmet"
    },
    {
        "id": "1414",
        "name": "Germany Helmet"
    },
    {
        "id": "1415",
        "name": "Japan Helmet"
    },
    {
        "id": "1416",
        "name": "Netherlands Helmet"
    },
    {
        "id": "1462",
        "name": "New Zealand Helmet"
    },
    {
        "id": "1465",
        "name": "Portugal Helmet"
    },
    {
        "id": "1470",
        "name": "Russia Helmet"
    },
    {
        "id": "1417",
        "name": "South Africa Helmet"
    },
    {
        "id": "1483",
        "name": "Spain Helmet"
    },
    {
        "id": "1473",
        "name": "Sweden Helmet"
    },
    {
        "id": "1418",
        "name": "United Kingdom Helmet"
    },
    {
        "id": "1419",
        "name": "USA Helmet"
    },
    {
        "id": "1420",
        "name": "Wales Helmet"
    },
    {
        "id": "1506",
        "name": "Nintendo Switch Helmet"
    },
    {
        "id": "1507",
        "name": "PS4 Helmet"
    },
    {
        "id": "1127",
        "name": "Xbox Helmet"
    },
    {
        "id": "1183",
        "name": "#TeamRazer Shorts"
    },
    {
        "id": "164",
        "name": "Arboreal Community Shorts"
    },
    {
        "id": "91",
        "name": "Arboreal Master Pants"
    },
    {
        "id": "67",
        "name": "Arboreal Master Shorts"
    },
    {
        "id": "76",
        "name": "Arboreal Novice Pants"
    },
    {
        "id": "78",
        "name": "Arboreal Novice Shorts"
    },
    {
        "id": "77",
        "name": "Arboreal Pro Pants"
    },
    {
        "id": "79",
        "name": "Arboreal Pro Shorts"
    },
    {
        "id": "1233",
        "name": "Cashcow Pants"
    },
    {
        "id": "1323",
        "name": "Arrow Pants"
    },
    {
        "id": "1676",
        "name": "Aura Pants"
    },
    {
        "id": "1610",
        "name": "Bronze Pants"
    },
    {
        "id": "1684",
        "name": "Bullion Pants"
    },
    {
        "id": "1317",
        "name": "Contrast Pants"
    },
    {
        "id": "1679",
        "name": "Cycledelic Pants"
    },
    {
        "id": "1443",
        "name": "Glocksee Pants"
    },
    {
        "id": "1608",
        "name": "Gold Pants"
    },
    {
        "id": "1334",
        "name": "Headshot Pants"
    },
    {
        "id": "1329",
        "name": "Hellwave Pants"
    },
    {
        "id": "1381",
        "name": "High Voltage Pants"
    },
    {
        "id": "1382",
        "name": "High Voltage Shorts"
    },
    {
        "id": "1668",
        "name": "Igloo Pants"
    },
    {
        "id": "1349",
        "name": "Poison Dart Pants"
    },
    {
        "id": "1359",
        "name": "Rascal Pants"
    },
    {
        "id": "1367",
        "name": "RIDE Shorts"
    },
    {
        "id": "1609",
        "name": "Silver Pants"
    },
    {
        "id": "1372",
        "name": "Zeron Pants"
    },
    {
        "id": "1373",
        "name": "Zeron Shorts"
    },
    {
        "id": "1544",
        "name": "Custom 01 Long Pants"
    },
    {
        "id": "1554",
        "name": "Custom 01 Short Shorts"
    },
    {
        "id": "1545",
        "name": "Custom 02 Long Pants"
    },
    {
        "id": "1555",
        "name": "Custom 02 Short Shorts"
    },
    {
        "id": "1546",
        "name": "Custom 03 Long Pants"
    },
    {
        "id": "1556",
        "name": "Custom 03 Short Shorts"
    },
    {
        "id": "1547",
        "name": "Custom 04 Long Pants"
    },
    {
        "id": "1557",
        "name": "Custom 04 Short Shorts"
    },
    {
        "id": "1548",
        "name": "Custom 05 Long Pants"
    },
    {
        "id": "1558",
        "name": "Custom 05 Short Shorts"
    },
    {
        "id": "1549",
        "name": "Custom 06 Long Pants"
    },
    {
        "id": "1559",
        "name": "Custom 06 Short Shorts"
    },
    {
        "id": "1550",
        "name": "Custom 07 Long Pants"
    },
    {
        "id": "1560",
        "name": "Custom 07 Short Shorts"
    },
    {
        "id": "1551",
        "name": "Custom 08 Long Pants"
    },
    {
        "id": "1561",
        "name": "Custom 08 Short Shorts"
    },
    {
        "id": "1552",
        "name": "Custom 09 Long Pants"
    },
    {
        "id": "1562",
        "name": "Custom 09 Short Shorts"
    },
    {
        "id": "1553",
        "name": "Custom 10 Long Pants"
    },
    {
        "id": "1563",
        "name": "Custom 10 Short Shorts"
    },
    {
        "id": "1210",
        "name": "Denim All American Pants"
    },
    {
        "id": "1212",
        "name": "Denim Dijon Pants"
    },
    {
        "id": "1211",
        "name": "Denim Herringbone Pants"
    },
    {
        "id": "1213",
        "name": "Denim Vintage Pants"
    },
    {
        "id": "1078",
        "name": "Descenders Legendary Pants"
    },
    {
        "id": "1355",
        "name": "Descenders Master Pants"
    },
    {
        "id": "1073",
        "name": "Descenders Pants"
    },
    {
        "id": "1005",
        "name": "Liquicity Pants"
    },
    {
        "id": "1011",
        "name": "No More Robots Pants"
    },
    {
        "id": "1019",
        "name": "Power Up Audio Pants"
    },
    {
        "id": "1015",
        "name": "RageSquid Pants"
    },
    {
        "id": "166",
        "name": "Enemy Community Shorts"
    },
    {
        "id": "106",
        "name": "Enemy Master Pants"
    },
    {
        "id": "61",
        "name": "Enemy Master Shorts"
    },
    {
        "id": "107",
        "name": "Enemy Novice Pants"
    },
    {
        "id": "59",
        "name": "Enemy Novice Shorts"
    },
    {
        "id": "108",
        "name": "Enemy Pro Pants"
    },
    {
        "id": "60",
        "name": "Enemy Pro Shorts"
    },
    {
        "id": "17",
        "name": "Ether Basic Pants"
    },
    {
        "id": "18",
        "name": "Ether Basic Shorts"
    },
    {
        "id": "130",
        "name": "Ether Caution Orange Pants"
    },
    {
        "id": "138",
        "name": "Ether Caution Orange Shorts"
    },
    {
        "id": "127",
        "name": "Ether Flash Yellow Pants"
    },
    {
        "id": "135",
        "name": "Ether Flash Yellow Shorts"
    },
    {
        "id": "128",
        "name": "Ether Fury Red Pants"
    },
    {
        "id": "136",
        "name": "Ether Fury Red Shorts"
    },
    {
        "id": "131",
        "name": "Ether Lime Green Pants"
    },
    {
        "id": "139",
        "name": "Ether Lime Green Shorts"
    },
    {
        "id": "132",
        "name": "Ether Marvelous Magenta Pants"
    },
    {
        "id": "140",
        "name": "Ether Marvelous Magenta Shorts"
    },
    {
        "id": "126",
        "name": "Ether Onyx Black Pants"
    },
    {
        "id": "134",
        "name": "Ether Onyx Black Shorts"
    },
    {
        "id": "129",
        "name": "Ether Triumph Blue Pants"
    },
    {
        "id": "137",
        "name": "Ether Triumph Blue Shorts"
    },
    {
        "id": "1054",
        "name": "FortyTwo Amber Pants"
    },
    {
        "id": "1103",
        "name": "FortyTwo Apex Pants"
    },
    {
        "id": "1055",
        "name": "FortyTwo Apple Pants"
    },
    {
        "id": "1056",
        "name": "FortyTwo Aqua Pants"
    },
    {
        "id": "1053",
        "name": "FortyTwo Cobalt Pants"
    },
    {
        "id": "1106",
        "name": "FortyTwo Crank Pants"
    },
    {
        "id": "1051",
        "name": "FortyTwo Frost Pants"
    },
    {
        "id": "1057",
        "name": "FortyTwo Neon Pants"
    },
    {
        "id": "1062",
        "name": "FortyTwo Rasta Pants"
    },
    {
        "id": "1104",
        "name": "FortyTwo Rival Pants"
    },
    {
        "id": "1058",
        "name": "FortyTwo Royal Pants"
    },
    {
        "id": "1105",
        "name": "FortyTwo Ultra Pants"
    },
    {
        "id": "1052",
        "name": "FortyTwo Vermillion Pants"
    },
    {
        "id": "173",
        "name": "High Rollers Pants"
    },
    {
        "id": "174",
        "name": "High Rollers Shorts"
    },
    {
        "id": "168",
        "name": "Kinetic Community Shorts"
    },
    {
        "id": "38",
        "name": "Kinetic Master Pants"
    },
    {
        "id": "42",
        "name": "Kinetic Master Shorts"
    },
    {
        "id": "36",
        "name": "Kinetic Novice Pants"
    },
    {
        "id": "40",
        "name": "Kinetic Novice Shorts"
    },
    {
        "id": "37",
        "name": "Kinetic Pro Pants"
    },
    {
        "id": "41",
        "name": "Kinetic Pro Shorts"
    },
    {
        "id": "1274",
        "name": "Lux Banana Pants"
    },
    {
        "id": "1278",
        "name": "Lux Descent Pants"
    },
    {
        "id": "1277",
        "name": "Lux District Pants"
    },
    {
        "id": "1276",
        "name": "Lux Lantern Pants"
    },
    {
        "id": "1275",
        "name": "Lux Legacy Pants"
    },
    {
        "id": "1125",
        "name": "Mixer Pants"
    },
    {
        "id": "1448",
        "name": "Mod.io Pants"
    },
    {
        "id": "1500",
        "name": "Dogtorque 2019 Pants"
    },
    {
        "id": "1498",
        "name": "KingKrautz 2019 Pants"
    },
    {
        "id": "1499",
        "name": "Spe 2019 Pants"
    },
    {
        "id": "1454",
        "name": "Australia Pants"
    },
    {
        "id": "1484",
        "name": "Austria Pants"
    },
    {
        "id": "1421",
        "name": "Canada Pants"
    },
    {
        "id": "1422",
        "name": "China Pants"
    },
    {
        "id": "1477",
        "name": "Estonia Pants"
    },
    {
        "id": "1458",
        "name": "Finland Pants"
    },
    {
        "id": "1423",
        "name": "France Pants"
    },
    {
        "id": "1424",
        "name": "Germany Pants"
    },
    {
        "id": "1425",
        "name": "Japan Pants"
    },
    {
        "id": "1426",
        "name": "Netherlands Pants"
    },
    {
        "id": "1460",
        "name": "New Zealand Pants"
    },
    {
        "id": "1466",
        "name": "Portugal Pants"
    },
    {
        "id": "1469",
        "name": "Russia Pants"
    },
    {
        "id": "1427",
        "name": "South Africa Pants"
    },
    {
        "id": "1485",
        "name": "Spain Pants"
    },
    {
        "id": "1474",
        "name": "Sweden Pants"
    },
    {
        "id": "1428",
        "name": "United Kingdom Pants"
    },
    {
        "id": "1429",
        "name": "USA Pants"
    },
    {
        "id": "1430",
        "name": "Wales Pants"
    },
    {
        "id": "1508",
        "name": "Nintendo Switch Pants"
    },
    {
        "id": "1626",
        "name": "Polka Dots Bear Pants"
    },
    {
        "id": "1623",
        "name": "Polka Dots Dino Pants"
    },
    {
        "id": "1641",
        "name": "Polka Dots Flamingo Pants"
    },
    {
        "id": "1629",
        "name": "Polka Dots Giraffe Pants"
    },
    {
        "id": "145",
        "name": "Polka Dots Orange Pants"
    },
    {
        "id": "1635",
        "name": "Polka Dots Penguin Pants"
    },
    {
        "id": "154",
        "name": "Polka Dots Pink Pants"
    },
    {
        "id": "153",
        "name": "Polka Dots Purple Pants"
    },
    {
        "id": "1632",
        "name": "Polka Dots Rhino Pants"
    },
    {
        "id": "1638",
        "name": "Polka Dots Shark Pants"
    },
    {
        "id": "1644",
        "name": "Polka Dots Unicorn Pants"
    },
    {
        "id": "155",
        "name": "Polka Dots Yellow Pants"
    },
    {
        "id": "1509",
        "name": "PS4 Pants"
    },
    {
        "id": "1230",
        "name": "Skeleton Pants"
    },
    {
        "id": "1614",
        "name": "War Child Shorts"
    },
    {
        "id": "1126",
        "name": "Xbox Pants"
    },
    {
        "id": "1182",
        "name": "#TeamRazer Jersey Short"
    },
    {
        "id": "1176",
        "name": "Arboreal Best Team Jersey Short"
    },
    {
        "id": "1252",
        "name": "Arboreal Christmas Jersey Long"
    },
    {
        "id": "163",
        "name": "Arboreal Community Jersey Short"
    },
    {
        "id": "86",
        "name": "Arboreal Master Jersey Long"
    },
    {
        "id": "89",
        "name": "Arboreal Master Jersey Short"
    },
    {
        "id": "87",
        "name": "Arboreal Novice Jersey Long"
    },
    {
        "id": "90",
        "name": "Arboreal Novice Jersey Short"
    },
    {
        "id": "88",
        "name": "Arboreal Pro Jersey Long"
    },
    {
        "id": "75",
        "name": "Arboreal Pro Jersey Short"
    },
    {
        "id": "1092",
        "name": "Beta Jersey Short"
    },
    {
        "id": "1234",
        "name": "Cashcow Jersey Long"
    },
    {
        "id": "1179",
        "name": "Chiq Bronze Jersey Short"
    },
    {
        "id": "1181",
        "name": "Chiq Gold Jersey Short"
    },
    {
        "id": "1618",
        "name": "Chiq Hawaii Blue Jersey Short"
    },
    {
        "id": "1616",
        "name": "Chiq Hawaii Green Jersey Short"
    },
    {
        "id": "1617",
        "name": "Chiq Hawaii Red Jersey Short"
    },
    {
        "id": "1180",
        "name": "Chiq Silver Jersey Short"
    },
    {
        "id": "1178",
        "name": "Chiq Tuxedo Jersey Short"
    },
    {
        "id": "1324",
        "name": "Arrow Jersey Long"
    },
    {
        "id": "1677",
        "name": "Aura Jersey Long"
    },
    {
        "id": "1206",
        "name": "Baebale Jersey Short"
    },
    {
        "id": "1207",
        "name": "Bossfence Jersey Short"
    },
    {
        "id": "1613",
        "name": "Bronze Jersey Long"
    },
    {
        "id": "1685",
        "name": "Bullion Jersey Long"
    },
    {
        "id": "1318",
        "name": "Contrast Jersey Long"
    },
    {
        "id": "1319",
        "name": "Contrast Jersey Short"
    },
    {
        "id": "1680",
        "name": "Cycledelic Jersey Long"
    },
    {
        "id": "1444",
        "name": "Glocksee Jersey Long"
    },
    {
        "id": "1611",
        "name": "Gold Jersey Long"
    },
    {
        "id": "1335",
        "name": "Headshot Jersey Long"
    },
    {
        "id": "1330",
        "name": "Hellwave Jersey Long"
    },
    {
        "id": "1383",
        "name": "High Voltage Jersey Long"
    },
    {
        "id": "1384",
        "name": "High Voltage Jersey Short"
    },
    {
        "id": "1669",
        "name": "Igloo Jersey Long"
    },
    {
        "id": "1199",
        "name": "Jeroendumb Jersey Short"
    },
    {
        "id": "1200",
        "name": "Lex Jersey Short"
    },
    {
        "id": "1205",
        "name": "No More Robots Jersey Short"
    },
    {
        "id": "1201",
        "name": "Pip Jersey Short"
    },
    {
        "id": "1350",
        "name": "Poison Dart Jersey Long"
    },
    {
        "id": "1204",
        "name": "RageSquid Jersey Short"
    },
    {
        "id": "1360",
        "name": "Rascal Jersey Long"
    },
    {
        "id": "1368",
        "name": "RIDE Jersey Short"
    },
    {
        "id": "1208",
        "name": "Rockdodge Jersey Short"
    },
    {
        "id": "1202",
        "name": "Roel Jersey Short"
    },
    {
        "id": "1203",
        "name": "Rosie Jersey Short"
    },
    {
        "id": "1612",
        "name": "Silver Jersey Long"
    },
    {
        "id": "1670",
        "name": "Toxic Jersey Short"
    },
    {
        "id": "1096",
        "name": "Uwotmate Jersey Short"
    },
    {
        "id": "1450",
        "name": "Wheelieking Jersey Short"
    },
    {
        "id": "1374",
        "name": "Zeron Jersey Long"
    },
    {
        "id": "1375",
        "name": "Zeron Jersey Short"
    },
    {
        "id": "1564",
        "name": "Custom 01 Long Jersey Long"
    },
    {
        "id": "1574",
        "name": "Custom 01 Short Jersey Short"
    },
    {
        "id": "1565",
        "name": "Custom 02 Long Jersey Long"
    },
    {
        "id": "1575",
        "name": "Custom 02 Short Jersey Short"
    },
    {
        "id": "1566",
        "name": "Custom 03 Long Jersey Long"
    },
    {
        "id": "1576",
        "name": "Custom 03 Short Jersey Short"
    },
    {
        "id": "1567",
        "name": "Custom 04 Long Jersey Long"
    },
    {
        "id": "1577",
        "name": "Custom 04 Short Jersey Short"
    },
    {
        "id": "1568",
        "name": "Custom 05 Long Jersey Long"
    },
    {
        "id": "1578",
        "name": "Custom 05 Short Jersey Short"
    },
    {
        "id": "1569",
        "name": "Custom 06 Long Jersey Long"
    },
    {
        "id": "1579",
        "name": "Custom 06 Short Jersey Short"
    },
    {
        "id": "1570",
        "name": "Custom 07 Long Jersey Long"
    },
    {
        "id": "1580",
        "name": "Custom 07 Short Jersey Short"
    },
    {
        "id": "1571",
        "name": "Custom 08 Long Jersey Long"
    },
    {
        "id": "1581",
        "name": "Custom 08 Short Jersey Short"
    },
    {
        "id": "1572",
        "name": "Custom 09 Long Jersey Long"
    },
    {
        "id": "1582",
        "name": "Custom 09 Short Jersey Short"
    },
    {
        "id": "1573",
        "name": "Custom 10 Long Jersey Long"
    },
    {
        "id": "1583",
        "name": "Custom 10 Short Jersey Short"
    },
    {
        "id": "1254",
        "name": "Descenders Christmas Jersey Long"
    },
    {
        "id": "1072",
        "name": "Descenders Jersey Long"
    },
    {
        "id": "1079",
        "name": "Descenders Legendary Jersey Long"
    },
    {
        "id": "1356",
        "name": "Descenders Master Jersey Long"
    },
    {
        "id": "1006",
        "name": "Liquicity Jersey Long"
    },
    {
        "id": "1010",
        "name": "No More Robots Jersey Long"
    },
    {
        "id": "1018",
        "name": "Power Up Audio Jersey Long"
    },
    {
        "id": "1013",
        "name": "RageSquid Jersey Long"
    },
    {
        "id": "1175",
        "name": "Enemy Best Team Jersey Short"
    },
    {
        "id": "1248",
        "name": "Enemy Christmas Jersey Long"
    },
    {
        "id": "165",
        "name": "Enemy Community Jersey Short"
    },
    {
        "id": "100",
        "name": "Enemy Master Jersey Long"
    },
    {
        "id": "103",
        "name": "Enemy Master Jersey Short"
    },
    {
        "id": "101",
        "name": "Enemy Novice Jersey Long"
    },
    {
        "id": "104",
        "name": "Enemy Novice Jersey Short"
    },
    {
        "id": "102",
        "name": "Enemy Pro Jersey Long"
    },
    {
        "id": "105",
        "name": "Enemy Pro Jersey Short"
    },
    {
        "id": "15",
        "name": "Ether Basic Jersey Long"
    },
    {
        "id": "16",
        "name": "Ether Basic Jersey Short"
    },
    {
        "id": "114",
        "name": "Ether Caution Orange Jersey Long"
    },
    {
        "id": "122",
        "name": "Ether Caution Orange Jersey Short"
    },
    {
        "id": "111",
        "name": "Ether Flash Yellow Jersey Long"
    },
    {
        "id": "119",
        "name": "Ether Flash Yellow Jersey Short"
    },
    {
        "id": "112",
        "name": "Ether Fury Red Jersey Long"
    },
    {
        "id": "120",
        "name": "Ether Fury Red Jersey Short"
    },
    {
        "id": "115",
        "name": "Ether Lime Green Jersey Long"
    },
    {
        "id": "123",
        "name": "Ether Lime Green Jersey Short"
    },
    {
        "id": "116",
        "name": "Ether Marvelous Magenta Jersey Long"
    },
    {
        "id": "124",
        "name": "Ether Marvelous Magenta Jersey Short"
    },
    {
        "id": "110",
        "name": "Ether Onyx Black Jersey Long"
    },
    {
        "id": "118",
        "name": "Ether Onyx Black Jersey Short"
    },
    {
        "id": "113",
        "name": "Ether Triumph Blue Jersey Long"
    },
    {
        "id": "121",
        "name": "Ether Triumph Blue Jersey Short"
    },
    {
        "id": "1209",
        "name": "Foodz Beer Jersey Short"
    },
    {
        "id": "1652",
        "name": "Foodz Candy Bar Jersey Short"
    },
    {
        "id": "176",
        "name": "Foodz Fries Jersey Short"
    },
    {
        "id": "175",
        "name": "Foodz Hamburger Jersey Short"
    },
    {
        "id": "178",
        "name": "Foodz Hotdog Jersey Short"
    },
    {
        "id": "1091",
        "name": "Foodz Lemons Jersey Short"
    },
    {
        "id": "177",
        "name": "Foodz Pepperoni Jersey Short"
    },
    {
        "id": "1122",
        "name": "Fool Jersey Long"
    },
    {
        "id": "1045",
        "name": "FortyTwo Amber Jersey Long"
    },
    {
        "id": "1099",
        "name": "FortyTwo Apex Jersey Long"
    },
    {
        "id": "1046",
        "name": "FortyTwo Apple Jersey Long"
    },
    {
        "id": "1047",
        "name": "FortyTwo Aqua Jersey Long"
    },
    {
        "id": "1044",
        "name": "FortyTwo Cobalt Jersey Long"
    },
    {
        "id": "1102",
        "name": "FortyTwo Crank Jersey Long"
    },
    {
        "id": "1042",
        "name": "FortyTwo Frost Jersey Long"
    },
    {
        "id": "1048",
        "name": "FortyTwo Neon Jersey Long"
    },
    {
        "id": "1061",
        "name": "FortyTwo Rasta Jersey Long"
    },
    {
        "id": "1100",
        "name": "FortyTwo Rival Jersey Long"
    },
    {
        "id": "1049",
        "name": "FortyTwo Royal Jersey Long"
    },
    {
        "id": "1101",
        "name": "FortyTwo Ultra Jersey Long"
    },
    {
        "id": "1043",
        "name": "FortyTwo Vermillion Jersey Long"
    },
    {
        "id": "1243",
        "name": "HAOW Black Jersey Short"
    },
    {
        "id": "1240",
        "name": "HAOW Navy Jersey Short"
    },
    {
        "id": "1241",
        "name": "HAOW Noir Jersey Short"
    },
    {
        "id": "1242",
        "name": "HAOW Vintage Jersey Short"
    },
    {
        "id": "171",
        "name": "High Rollers Jersey Long"
    },
    {
        "id": "172",
        "name": "High Rollers Jersey Short"
    },
    {
        "id": "1286",
        "name": "Hipwood Blue Jersey Long"
    },
    {
        "id": "1289",
        "name": "Hipwood Brown Jersey Long"
    },
    {
        "id": "1287",
        "name": "Hipwood Green Jersey Long"
    },
    {
        "id": "1284",
        "name": "Hipwood Red Jersey Long"
    },
    {
        "id": "1285",
        "name": "Hipwood White Jersey Long"
    },
    {
        "id": "1288",
        "name": "Hipwood Yellow Jersey Long"
    },
    {
        "id": "1139",
        "name": "Indiebox Arboreal Jersey Short"
    },
    {
        "id": "1140",
        "name": "Indiebox Descenders Jersey Short"
    },
    {
        "id": "1137",
        "name": "Indiebox Enemy Jersey Short"
    },
    {
        "id": "1138",
        "name": "Indiebox Kinetic Jersey Short"
    },
    {
        "id": "1177",
        "name": "Kinetic Best Team Jersey Short"
    },
    {
        "id": "1250",
        "name": "Kinetic Christmas Jersey Long"
    },
    {
        "id": "167",
        "name": "Kinetic Community Jersey Short"
    },
    {
        "id": "32",
        "name": "Kinetic Master Jersey Long"
    },
    {
        "id": "35",
        "name": "Kinetic Master Jersey Short"
    },
    {
        "id": "30",
        "name": "Kinetic Novice Jersey Long"
    },
    {
        "id": "33",
        "name": "Kinetic Novice Jersey Short"
    },
    {
        "id": "31",
        "name": "Kinetic Pro Jersey Long"
    },
    {
        "id": "34",
        "name": "Kinetic Pro Jersey Short"
    },
    {
        "id": "1269",
        "name": "Lux Banana Jersey Long"
    },
    {
        "id": "1273",
        "name": "Lux Descent Jersey Long"
    },
    {
        "id": "1272",
        "name": "Lux District Jersey Long"
    },
    {
        "id": "1271",
        "name": "Lux Lantern Jersey Long"
    },
    {
        "id": "1270",
        "name": "Lux Legacy Jersey Long"
    },
    {
        "id": "1095",
        "name": "Mixer Jersey Short"
    },
    {
        "id": "1449",
        "name": "Mod.io Jersey Long"
    },
    {
        "id": "1502",
        "name": "Dogtorque 2019 Jersey Short"
    },
    {
        "id": "1503",
        "name": "KingKrautz 2019 Jersey Long"
    },
    {
        "id": "1501",
        "name": "Spe 2019 Jersey Short"
    },
    {
        "id": "1455",
        "name": "Australia Jersey Long"
    },
    {
        "id": "1486",
        "name": "Austria Jersey Long"
    },
    {
        "id": "1431",
        "name": "Canada Jersey Long"
    },
    {
        "id": "1432",
        "name": "China Jersey Long"
    },
    {
        "id": "1476",
        "name": "Estonia Jersey Long"
    },
    {
        "id": "1459",
        "name": "Finland Jersey Long"
    },
    {
        "id": "1433",
        "name": "France Jersey Long"
    },
    {
        "id": "1434",
        "name": "Germany Jersey Long"
    },
    {
        "id": "1435",
        "name": "Japan Jersey Long"
    },
    {
        "id": "1436",
        "name": "Netherlands Jersey Long"
    },
    {
        "id": "1461",
        "name": "New Zealand Jersey Long"
    },
    {
        "id": "1467",
        "name": "Portugal Jersey Long"
    },
    {
        "id": "1468",
        "name": "Russia Jersey Long"
    },
    {
        "id": "1437",
        "name": "South Africa Jersey Long"
    },
    {
        "id": "1487",
        "name": "Spain Jersey Long"
    },
    {
        "id": "1475",
        "name": "Sweden Jersey Long"
    },
    {
        "id": "1438",
        "name": "United Kingdom Jersey Long"
    },
    {
        "id": "1439",
        "name": "USA Jersey Long"
    },
    {
        "id": "1440",
        "name": "Wales Jersey Long"
    },
    {
        "id": "1510",
        "name": "Nintendo Switch Jersey Short"
    },
    {
        "id": "1625",
        "name": "Polka Dots Bear Jersey Long"
    },
    {
        "id": "1622",
        "name": "Polka Dots Dino Jersey Long"
    },
    {
        "id": "1642",
        "name": "Polka Dots Flamingo Jersey Long"
    },
    {
        "id": "1630",
        "name": "Polka Dots Giraffe Jersey Long"
    },
    {
        "id": "143",
        "name": "Polka Dots Orange Jersey Long"
    },
    {
        "id": "1636",
        "name": "Polka Dots Penguin Jersey Long"
    },
    {
        "id": "148",
        "name": "Polka Dots Pink Jersey Long"
    },
    {
        "id": "147",
        "name": "Polka Dots Purple Jersey Long"
    },
    {
        "id": "1633",
        "name": "Polka Dots Rhino Jersey Long"
    },
    {
        "id": "1639",
        "name": "Polka Dots Shark Jersey Long"
    },
    {
        "id": "1645",
        "name": "Polka Dots Unicorn Jersey Long"
    },
    {
        "id": "149",
        "name": "Polka Dots Yellow Jersey Long"
    },
    {
        "id": "1511",
        "name": "PS4 Jersey Short"
    },
    {
        "id": "1121",
        "name": "Scrape Dirty Camo Jersey Long"
    },
    {
        "id": "1120",
        "name": "Scrape Ride and Destroy Jersey Long"
    },
    {
        "id": "1119",
        "name": "Scrape Ride and Destroy Jersey Short"
    },
    {
        "id": "1086",
        "name": "AdmiralBulldog Jersey Short"
    },
    {
        "id": "1163",
        "name": "Bay Area Buggs Jersey Short"
    },
    {
        "id": "1236",
        "name": "CivRyan Jersey Short"
    },
    {
        "id": "1085",
        "name": "Draegast Jersey Short"
    },
    {
        "id": "1229",
        "name": "Firekitten Jersey Long"
    },
    {
        "id": "1451",
        "name": "Funhaus Jersey Short"
    },
    {
        "id": "1068",
        "name": "Golden Times Jersey Short"
    },
    {
        "id": "1090",
        "name": "Jackhuddo Jersey Short"
    },
    {
        "id": "1093",
        "name": "Jacksepticeye Jersey Short"
    },
    {
        "id": "1083",
        "name": "MANvsGAME Jersey Short"
    },
    {
        "id": "1081",
        "name": "NLSS Jersey Short"
    },
    {
        "id": "1069",
        "name": "Murder Bunny Jersey Short"
    },
    {
        "id": "1082",
        "name": "RockLeeSmile Jersey Short"
    },
    {
        "id": "1512",
        "name": "Sam Tabor Gaming Jersey Short"
    },
    {
        "id": "1094",
        "name": "Sodapoppin Jersey Short"
    },
    {
        "id": "1223",
        "name": "Something Rad Jersey Short"
    },
    {
        "id": "1084",
        "name": "Spamfish Jersey Short"
    },
    {
        "id": "1352",
        "name": "Toasty Ghost Jersey Short"
    },
    {
        "id": "1067",
        "name": "Velcro Fly Jersey Short"
    },
    {
        "id": "1231",
        "name": "Skeleton Jersey Long"
    },
    {
        "id": "1488",
        "name": "VERY DECENT Jersey Short"
    },
    {
        "id": "1615",
        "name": "War Child Jersey Short"
    },
    {
        "id": "1097",
        "name": "Xbox One Jersey Short"
    }
]
