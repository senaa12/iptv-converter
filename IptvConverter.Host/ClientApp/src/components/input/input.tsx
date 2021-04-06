import React from 'react';
import classNames from 'classnames';
import Spinner, { ClipSpinner } from '../spinner';

interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {
    loading?: boolean
}

const input = React.forwardRef(({ loading, ...rest }: InputProps, ref: any) => {
    const className = classNames(rest.className, {
    });

    return (
        <div style={{ position: 'relative' }}>
            <input
                {...rest}
                ref={ref}
                disabled={!!rest.disabled || !!loading}
                className={className}
            />
            {!!loading && 
                <div style={{ position: 'absolute', top: 0, bottom: 0, left: 0, right: 0 }}>
					<Spinner>
						<ClipSpinner size={20} color='#fff' />
					</Spinner>
			    </div>}
        </div>
    )
})

export default input;