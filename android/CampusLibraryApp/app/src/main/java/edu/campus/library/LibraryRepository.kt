package edu.campus.library

import com.google.gson.annotations.SerializedName
import okhttp3.OkHttpClient
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.POST
import java.util.concurrent.TimeUnit

data class Book(val id: Int, val title: String, val author: String,
    @SerializedName("isbn") val isbn: String, val availableCopies: Int, val isAvailable: Boolean)
data class Member(val id: Int, val fullName: String, val email: String)
data class Loan(val id: Int, val bookId: Int, val bookTitle: String, val memberId: Int,
    val memberName: String, val borrowedAt: String, val returnedAt: String?)
data class BorrowRequest(val bookId: Int, val memberId: Int)

interface LibraryApi {
    @GET("books") suspend fun books(): List<Book>
    @GET("members") suspend fun members(): List<Member>
    @GET("loans") suspend fun loans(): List<Loan>
    @POST("loans") suspend fun borrow(@Body request: BorrowRequest): Loan
}

// The initial API has no identity endpoint. Member filtering is a development
// convenience, not access control; authentication must precede public deployment.
class LibraryRepository(private val api: LibraryApi) {
    suspend fun books() = api.books()
    suspend fun members() = api.members()
    suspend fun loans(memberId: Int) = api.loans().filter { it.memberId == memberId }
    suspend fun borrow(bookId: Int, memberId: Int) = api.borrow(BorrowRequest(bookId, memberId))

    companion object {
        fun create(): LibraryRepository {
            val client = OkHttpClient.Builder().connectTimeout(10, TimeUnit.SECONDS)
                .readTimeout(15, TimeUnit.SECONDS).callTimeout(20, TimeUnit.SECONDS).build()
            return LibraryRepository(Retrofit.Builder().baseUrl(BuildConfig.API_BASE_URL)
                .client(client).addConverterFactory(GsonConverterFactory.create()).build()
                .create(LibraryApi::class.java))
        }
    }
}
