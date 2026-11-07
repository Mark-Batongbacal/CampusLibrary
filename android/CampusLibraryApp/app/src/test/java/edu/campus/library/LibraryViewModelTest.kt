package edu.campus.library

import androidx.lifecycle.SavedStateHandle
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.test.*
import org.junit.After
import org.junit.Assert.*
import org.junit.Before
import org.junit.Test
import java.io.IOException

@OptIn(ExperimentalCoroutinesApi::class)
class LibraryViewModelTest {
    private val dispatcher = StandardTestDispatcher()
    @Before fun setup() { Dispatchers.setMain(dispatcher) }
    @After fun cleanup() { Dispatchers.resetMain() }

    private class FakeApi : LibraryApi {
        var fail = false
        var borrowCount = 0
        override suspend fun books(): List<Book> {
            if (fail) throw IOException("private host information")
            return listOf(Book(1, "Book", "Author", "123", 1, true))
        }
        override suspend fun members() = listOf(Member(1, "One", "one@example.test"), Member(2, "Two", "two@example.test"))
        override suspend fun loans() = listOf(loan(1), loan(2))
        override suspend fun borrow(request: BorrowRequest): Loan { borrowCount++; return loan(request.memberId) }
        fun loan(memberId: Int) = Loan(memberId, 1, "Book", memberId, "Member", "2026-11-07T00:00:00Z", null)
    }

    @Test fun selectionFiltersHistoryAndSurvivesRecreation() = runTest(dispatcher) {
        val api = FakeApi()
        val saved = SavedStateHandle()
        val model = LibraryViewModel(LibraryRepository(api), saved)
        advanceUntilIdle()
        assertNull(model.state.value.memberId)
        model.selectMember(2)
        advanceUntilIdle()
        assertEquals(listOf(2), model.state.value.loans.map { it.memberId })
        val recreated = LibraryViewModel(LibraryRepository(api), saved)
        advanceUntilIdle()
        assertEquals(2, recreated.state.value.memberId)
        assertEquals(listOf(2), recreated.state.value.loans.map { it.memberId })
    }

    @Test fun borrowRequiresMemberAndPreventsDuplicateSubmission() = runTest(dispatcher) {
        val api = FakeApi()
        val model = LibraryViewModel(LibraryRepository(api), SavedStateHandle())
        advanceUntilIdle()
        model.borrow(1)
        assertEquals(0, api.borrowCount)
        model.selectMember(1)
        advanceUntilIdle()
        model.borrow(1)
        model.borrow(1)
        advanceUntilIdle()
        assertEquals(1, api.borrowCount)
        assertFalse(model.state.value.books.single().isAvailable)
        assertNotNull(model.state.value.notice)
    }

    @Test fun connectionFailureIsSafeAndRetryRecovers() = runTest(dispatcher) {
        val api = FakeApi().apply { fail = true }
        val model = LibraryViewModel(LibraryRepository(api), SavedStateHandle())
        advanceUntilIdle()
        assertFalse(model.state.value.loading)
        assertTrue(model.state.value.error!!.contains("Cannot reach"))
        assertFalse(model.state.value.error!!.contains("private"))
        api.fail = false
        model.refresh()
        advanceUntilIdle()
        assertNull(model.state.value.error)
        assertEquals(1, model.state.value.books.size)
    }
}
