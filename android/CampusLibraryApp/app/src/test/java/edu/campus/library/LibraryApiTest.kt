package edu.campus.library

import kotlinx.coroutines.test.runTest
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.Assert.*
import org.junit.Test
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

class LibraryApiTest {
    @Test fun readsAspNetCamelCaseAndSendsBorrowContract() = runTest {
        MockWebServer().use { server ->
            server.start()
            val api = Retrofit.Builder().baseUrl(server.url("/api/"))
                .addConverterFactory(GsonConverterFactory.create()).build().create(LibraryApi::class.java)
            server.enqueue(MockResponse().setBody("""[{"id":1,"title":"Book","author":"Writer","isbn":"978123","availableCopies":1,"isAvailable":false}]"""))
            val book = api.books().single()
            assertEquals("978123", book.isbn)
            assertFalse(book.isAvailable)
            assertEquals("/api/books", server.takeRequest().path)
            server.enqueue(MockResponse().setResponseCode(201).setBody("""{"id":9,"bookId":1,"bookTitle":"Book","memberId":2,"memberName":"Member","borrowedAt":"2026-11-07T00:00:00Z","returnedAt":null}"""))
            assertEquals(9, api.borrow(BorrowRequest(1, 2)).id)
            val request = server.takeRequest()
            assertEquals("POST", request.method)
            assertEquals("/api/loans", request.path)
            assertEquals("{\"bookId\":1,\"memberId\":2}", request.body.readUtf8())
        }
    }
}
