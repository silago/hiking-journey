package com.silagonevermind.hikingjourney

import android.content.Intent
import android.os.Bundle
import android.util.Log
//import androidx.health.connect.client.HealthConnectClient
//import androidx.health.connect.client.PermissionController
//import androidx.health.connect.client.permission.HealthPermission
//import androidx.health.connect.client.records.StepsRecord
//import androidx.health.connect.client.request.ReadRecordsRequest
import androidx.core.app.ActivityCompat

//import androidx.health.connect.client.time.TimeRangeFilter
import java.time.Instant
import com.unity3d.player.UnityPlayerActivity;
import com.unity3d.player.UnityPlayer;

import android.net.Uri
import kotlinx.coroutines.DelicateCoroutinesApi
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch

class CustomUnityActivity : UnityPlayerActivity() {
        /*
    private var healthConnectClient: HealthConnectClient? = null


    override fun onCreate(savedInstanceState: Bundle?) {
    
        var providerPackageName = "com.google.android.apps.healthdata";
        var context = this.applicationContext; 


        val availabilityStatus = HealthConnectClient.getSdkStatus(context, providerPackageName)
        if (availabilityStatus == HealthConnectClient.SDK_UNAVAILABLE) {
            return // early return as there is no viable integration
        }
        if (availabilityStatus == HealthConnectClient.SDK_UNAVAILABLE_PROVIDER_UPDATE_REQUIRED) {
            // Optionally redirect to package installer to find a provider, for example:
            val uriString = "market://details?id=$providerPackageName&url=healthconnect%3A%2F%2Fonboarding"
            context.startActivity(
                Intent(Intent.ACTION_VIEW).apply {
                    setPackage("com.android.vending")
                    data = Uri.parse(uriString)
                    putExtra("overlay", true)
                    putExtra("callerId", context.packageName)
                }
            )
            return
        }
        
        healthConnectClient = HealthConnectClient.getOrCreate(context)


        // Calls UnityPlayerActivity.onCreate()
        super.onCreate(savedInstanceState)
        // Prints debug message to Logcat
        Log.d("OverrideActivity", "onCreate called!")
    }
        */

    /*
    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent) {
        Log.d("Health", requestCode.toString());
        Log.d("Health", resultCode.toString());
        Log.d("Health", data.toString());

        super.onActivityResult(requestCode, resultCode, data)
    }
     

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<String>,
        grantResults: IntArray
    ) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        Log.d("Health", grantResults.toString())
        Log.d("Health", permissions.toString())
    }

    fun requestStepPermission() {
        Log.d("Health", "request permission");

        val list = HashSet<String>()
        list.add(
            HealthPermission.getReadPermission(StepsRecord::class)
        )

        val permsIntent = PermissionController.createRequestPermissionResultContract().createIntent(
            applicationContext, list
        )

        ActivityCompat.requestPermissions(
            this,
            arrayOf(HealthPermission.getReadPermission(StepsRecord::class).toString()),
            1
        )

    }

    fun getStepsCount(from : String, to : String) {
        GlobalScope.launch {
            susGetStepsCount(from, to)
        }
    }

    suspend fun susGetStepsCount(from : String, to : String): Long {
        Log.d("Health","start stepscount");
        var startTime = Instant.parse(from);
        var endTime = Instant.parse(to);
        val response =
            this.healthConnectClient?.readRecords(
                ReadRecordsRequest<StepsRecord>(
                    timeRangeFilter = TimeRangeFilter.between(startTime, endTime)
                )
            )

        var result: Long = 0;
        if (response != null) {
            for (r in response.records) {
                result += r.count;
            }
            
            UnityPlayer.UnitySendMessage("MainActivityIteractor", "OnStepsCountReceived", result.toString())
            Log.d("Health",result.toString());
        } else {
            Log.e("Health", "Response is null")
        }

        return result;
    }
    */

    companion object {
        private const val TAG = "CustomUnityActivity"
    }
}