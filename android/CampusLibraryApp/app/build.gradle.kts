plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.plugin.compose")
}
val releaseUrl = providers.gradleProperty("API_BASE_URL").orElse("https://example.invalid/api/")
android {
    namespace = "edu.campus.library"
    compileSdk = 36
    defaultConfig {
        applicationId = "edu.campus.library"
        minSdk = 26
        targetSdk = 36
        versionCode = 1
        versionName = "1.0"
    }
    buildTypes {
        debug { buildConfigField("String", "API_BASE_URL", "\"http://10.0.2.2:5080/api/\"") }
        release {
            require(releaseUrl.get().startsWith("https://") && releaseUrl.get().endsWith("/")) { "API_BASE_URL must be an HTTPS URL ending in /" }
            buildConfigField("String", "API_BASE_URL", "\"${releaseUrl.get()}\"")
        }
    }
    buildFeatures { compose = true; buildConfig = true }
    compileOptions { sourceCompatibility = JavaVersion.VERSION_17; targetCompatibility = JavaVersion.VERSION_17 }
}
dependencies {
    implementation("androidx.activity:activity-compose:1.13.0")
    implementation("androidx.compose.material3:material3:1.4.0")
    implementation("androidx.lifecycle:lifecycle-viewmodel-compose:2.10.0")
    implementation("androidx.lifecycle:lifecycle-runtime-compose:2.10.0")
    implementation("com.squareup.retrofit2:retrofit:3.0.0")
    implementation("com.squareup.retrofit2:converter-gson:3.0.0")
    testImplementation("junit:junit:4.13.2")
    testImplementation("org.jetbrains.kotlinx:kotlinx-coroutines-test:1.10.2")
    testImplementation("com.squareup.okhttp3:mockwebserver:4.12.0")
}
