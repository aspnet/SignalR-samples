package com.microsoft.aspnet.signalr.samples.androidjavaclient;


import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.TextView;

import com.microsoft.aspnet.signalr.Action;
import com.microsoft.aspnet.signalr.HubConnection;
import com.microsoft.aspnet.signalr.HubConnectionBuilder;
import com.microsoft.aspnet.signalr.LogLevel;

import java.util.ArrayList;
import java.util.List;

public class MainActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        HubConnection hubConnection = new HubConnection("http://signalr-samples.azurewebsites.net/default");
        TextView textView = (TextView)findViewById(R.id.tvMain);
        ListView listView = (ListView)findViewById(R.id.lvMessages);
        Button sendButton = (Button)findViewById(R.id.bSend);
        EditText editText = (EditText)findViewById(R.id.etMessageText);

        List<String> messageList = new ArrayList<String>();
        ArrayAdapter<String> arrayAdapter = new ArrayAdapter<String>(MainActivity.this,
                android.R.layout.simple_list_item_1, messageList);
        listView.setAdapter(arrayAdapter);

        hubConnection.on("Send", (message)-> {
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    arrayAdapter.add(message);
                    arrayAdapter.notifyDataSetChanged();
                }
            });
        }, String.class);

        sendButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                String message = editText.getText().toString();
                editText.setText("");
                try {
                    hubConnection.send("Send", message);
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
        });

        try {
            hubConnection.start();
        } catch (Exception e) {
            e.printStackTrace();
            textView.setText("There was an error: " + e.getMessage());
        }
    }
}
