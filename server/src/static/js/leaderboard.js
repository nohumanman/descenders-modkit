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
        time: 0,
    },
    methods: {
        GetLeaderboard(trail_name, timestamp, spectated_by){
            $.get("/get-leaderboard", {
                "trail_name" : trail_name,
                "timestamp" : timestamp,
                "spectated_by": spectated_by
            }, function(data){
                app.leaderboard = data;
            });
            app.trail_name = trail_name;
            app.timestamp = parseFloat(timestamp);
        },
        secs_to_str(secs){
            secs = parseFloat(secs);
            d_mins = Math.floor(secs / 60);
            d_secs = Math.floor(secs % 60)
            fraction = secs * 1000;
            fraction = Math.round(fraction % 1000);
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
        }
    }
});

const queryString = window.location.search;
const urlParams = new URLSearchParams(queryString);
const trail = urlParams.get('trail_name')
const timestamp = urlParams.get('timestamp')
const spectated_by = urlParams.get('spectated_by')

app.GetLeaderboard(trail, timestamp, spectated_by);
