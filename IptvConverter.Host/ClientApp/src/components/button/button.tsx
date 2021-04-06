import React, { ButtonHTMLAttributes, useCallback } from 'react';
import Spinner, { ClipSpinner } from '../spinner';
import classNames from 'classnames';

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    loading?: boolean;
    textual?: boolean;
}

const button = ({ loading, textual, ...rest }: ButtonProps) => {
    const onButtonClick = useCallback((e: any) => {
        if(!!rest.disabled || !!loading) {
            return;
        }

        if(!!rest.onClick) {
            rest.onClick(e);
        }
    }, [rest.disabled, loading, rest.onClick]);

    const className = classNames(rest.className, {
        'textual': !!textual
    });
    return (
        <button 
            {...rest}
            disabled={rest.disabled || !!loading}
            onClick={onButtonClick}
            style={{ ...rest.style, position: 'relative' }} // position relative because of loading
            className={className}
        >
            {!!loading && 
                <div style={{ position: 'absolute', top: 0, bottom: 0, left: 0, right: 0 }}>
					<Spinner>
						<ClipSpinner size={20} color='#fff' />
					</Spinner>
			    </div>}
            {rest.children}
        </button>
    );
}

export default button;