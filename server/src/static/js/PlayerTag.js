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
        spectating: "",
    },
    methods: {
        updateSpectatedPlayer(){
            $.get("/api/spectating/get-time", data={"our_id": this.getSelf()}, function(data){
                app.spectating = data;
            });
        },
        getSelf(){
            return this.GetURLParameter("id");
        },
        GetURLParameter(sParam)
        {
            var sPageURL = window.location.search.substring(1);
            var sURLVariables = sPageURL.split('&');
            for (var i = 0; i < sURLVariables.length; i++) 
            {
                var sParameterName = sURLVariables[i].split('=');
                if (sParameterName[0] == sParam) 
                {
                    return sParameterName[1];
                }
            }
        }
    }
});

app.updateSpectatedPlayer();
setInterval(app.updateSpectatedPlayer, 1000);
