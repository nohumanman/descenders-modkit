var app = new Vue({
    el: '#app',
    delimiters: ['[[', ']]'],
    vuetify : new Vuetify({
        theme: { dark: true },
    }),
    data : {
        players: [],
        currentTime: 0,
        started: false,
        spectating: null
    },
    methods: {
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
        },
        getPlayer(id){
            app.players.forEach(player => {
                if (player.steam_id == id)
                    return player;
            });
        },
        updateTime(){
            if (app.started){
                if (app.start_time == 0 || app.start_time == undefined)
                    return;
                app.currentTime = Date.now()/1000 - app.start_time;
                if (app.currentTime < 0)
                    app.currentTime = 0;
            }
            else
                app.currentTime = app.start_time;
            if (app.currentTime == undefined)
                app.currentTime = 0;
        },
        updateTimeFromServer(){
            $.get("/api/spectating/get-time", data={"my_id": this.getSelf()}, function(data){
                app.started = data.started;
                app.start_time = data.time;
                setTimeout(function() {
                    app.updateTimeFromServer();
                }, 1000); 
            })
            .fail(function(){
                setTimeout(function() {
                    app.updateTimeFromServer();
                }, 1000); 
            });
        },
        getSelf(){
            const params = new URLSearchParams(window.location.search)
            return params.get('id')
        }
    }
});

const params = new URLSearchParams(window.location.search)
app.trail_name = params.get('trail_name')

setInterval(() => {
    app.updateTime();
}, 1);

app.updateTimeFromServer();