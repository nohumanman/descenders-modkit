var app = new Vue({
    el: '#app',
    delimiters: ['[[', ']]'],
    vuetify : new Vuetify({
        theme: { dark: true },
    }),
    data : {
        currentTime: 0,
        spectated_player: null,
        trail_name: "Snowbird Speed and Style"
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
        updateTime(){
            trails = app.spectated_player?.trails
            if (trails != null){
                trails.forEach(
                    function(data){
                        if (data.trail_name == app.trail_name){
                            if (data.started){
                                app.currentTime = Date.now()/1000 - data.time_started;
                                if (app.currentTime < 0)
                                    app.currentTime = 0;
                            }
                            else
                                app.currentTime = data.last_time;
                        }
                    }
                )
            }
        },
        getSpectatedInfo(){
            $.get("/get-spectated", function(data){
                app.spectated_player = data;
            })
        },
    }
});

app.startTime = Date.now();

const params = new URLSearchParams(window.location.search)
app.trail_name = params.get('trail_name')

setInterval(() => {
    app.updateTime();
}, 1);

setInterval(() =>{
    app.getSpectatedInfo();
}, 500)



