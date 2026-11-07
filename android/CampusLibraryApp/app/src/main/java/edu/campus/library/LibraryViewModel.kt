package edu.campus.library

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import retrofit2.HttpException
import java.io.IOException

data class LibraryState(
    val books: List<Book> = emptyList(), val members: List<Member> = emptyList(),
    val loans: List<Loan> = emptyList(), val memberId: Int? = null,
    val loading: Boolean = true, val borrowing: Boolean = false,
    val error: String? = null, val notice: String? = null
)

class LibraryViewModel(private val repository: LibraryRepository,
    private val saved: SavedStateHandle) : ViewModel() {
    private val mutable = MutableStateFlow(LibraryState(memberId = saved["memberId"]))
    val state = mutable.asStateFlow()
    private var refreshJob: Job? = null
    init { refresh() }

    fun selectMember(id: Int) {
        if (state.value.borrowing || state.value.members.none { it.id == id }) return
        saved["memberId"] = id
        mutable.update { it.copy(memberId = id, loans = emptyList(), notice = null) }
        refresh()
    }

    fun refresh() {
        if (state.value.borrowing) return
        refreshJob?.cancel()
        refreshJob = viewModelScope.launch {
            mutable.update { it.copy(loading = true, error = null) }
            try {
                val books = repository.books()
                val members = repository.members()
                val memberId = state.value.memberId?.takeIf { id -> members.any { it.id == id } }
                val loans = memberId?.let { repository.loans(it) } ?: emptyList()
                saved["memberId"] = memberId
                mutable.update { it.copy(books = books, members = members, memberId = memberId,
                    loans = loans, loading = false) }
            } catch (cancelled: CancellationException) { throw cancelled }
            catch (error: Exception) {
                mutable.update { it.copy(loading = false, error = message(error)) }
            }
        }
    }

    fun borrow(bookId: Int) {
        val current = state.value
        val memberId = current.memberId ?: return
        if (current.loading || current.borrowing || current.books.none { it.id == bookId && it.isAvailable }) return
        mutable.update { it.copy(borrowing = true, error = null, notice = null) }
        viewModelScope.launch {
            try {
                val loan = repository.borrow(bookId, memberId)
                mutable.update { it.copy(borrowing = false,
                    books = it.books.map { book -> if (book.id == bookId) book.copy(isAvailable = false) else book },
                    loans = listOf(loan) + it.loans,
                    notice = "Borrowed successfully. Your loan is in My loans.") }
            } catch (cancelled: CancellationException) { throw cancelled }
            catch (error: Exception) {
                mutable.update { it.copy(borrowing = false, error = message(error),
                    books = if (error is HttpException && error.code() == 409)
                        it.books.map { book -> if (book.id == bookId) book.copy(isAvailable = false) else book }
                    else it.books) }
            }
        }
    }

    private fun message(error: Exception) = when (error) {
        is IOException -> "Cannot reach the library. Check your connection and try again."
        is HttpException -> when (error.code()) {
            409 -> "This book is no longer available. Refresh the catalog to check availability."
            404 -> "The book or member no longer exists. Refresh and select a member again."
            else -> "The library could not complete the request. Please try again."
        }
        else -> "Could not load library information. Please try again."
    }
}
