var app = new Vue({
    el: '#app',
    delimiters: ['[[', ']]'],
    vuetify : new Vuetify({
        theme: {
            dark : {
                primary: '#1976D2',
                secondary: '#424242',
                accent: '#82B1FF',
                error: '#FF5252',
                info: '#2196F3',
                success: '#4CAF50',
                warning: '#FFC107',
            }
    },
    }),
    data : {
        leaderboard: [],
        trailName: '',
        timestamp: 0.0,
    },
    methods: {
        GetLeaderboard(trail_name, timestamp){
            $.get("/get-leaderboard", {
                "trail_name" : trail_name,
                "timestamp" : timestamp
            }, function(data){
                app.leaderboard = data;
            });
            app.trail_name = trail_name;
            app.timestamp = parseFloat(timestamp);
        },
        timeToStr(new_time) {
            intTime = parseFloat(new_time);
            minutes = Math.trunc(intTime / 60);
            if (minutes.toString().length == 1){
                minutes = "0" + minutes.toString();
            }
            seconds =  Math.trunc(intTime % 60);
            if (seconds.toString().length == 1){
                seconds = "0" + seconds.toString();
            }
            fraction = intTime * 1000;
            fraction =  Math.trunc(fraction % 1000).toString();
            while (fraction.toString().length < 3){
                fraction = "0" + fraction;
            }
            timeText = minutes.toString() + ":" + seconds.toString() + ":" + fraction.toString();
            return timeText.toString();
        }
    }
});

const queryString = window.location.search;
const urlParams = new URLSearchParams(queryString);
const trail = urlParams.get('trail_name')
const timestamp = urlParams.get('timestamp')


app.GetLeaderboard(trail, timestamp);
